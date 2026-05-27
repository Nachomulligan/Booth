// ============================================================
// CustomerPresenter.cs
// Maneja toda la representación visual del cliente y su dinero.
//
// El sistema de dinero ahora es físico:
//   - BillsContainer tiene slots de Image donde aparecen sprites
//   - Cada DenominationInstance mapea a un sprite via MoneyVisualConfig
//   - Los billetes falsos usan su propio sprite (detectables pero sutiles)
//   - Al pedir más, las piezas extra se agregan a los existentes
// ============================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Booth.Customers;

namespace Booth.Customers
{
    public class CustomerPresenter : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────
        [Header("Customer Visual")]
        [SerializeField] private Image    _customerSprite;
        [SerializeField] private Animator _customerAnimator; // opcional

        [Header("Money Display")]
        [Tooltip("Contenedor donde se instancian los sprites de billetes/monedas.")]
        [SerializeField] private RectTransform _billsContainer;

        [Tooltip("Prefab de un slot de dinero. Debe tener un componente Image.")]
        [SerializeField] private GameObject _moneySlotPrefab;

        [Tooltip("Config con sprites de billetes y monedas.")]
        [SerializeField] private MoneyVisualConfig _moneyVisualConfig;

        [Header("Sprite Config")]
        [SerializeField] private CustomerVisualConfig _visualConfig;

        [Header("Layout")]
        [Tooltip("Espaciado horizontal entre piezas de dinero.")]
        [SerializeField] private float _slotSpacing = 70f;
        [Tooltip("Offset Y para separar billetes (arriba) de monedas (abajo).")]
        [SerializeField] private float _coinRowOffsetY = -60f;

        [Header("Animation")]
        [SerializeField] private float _slideDuration  = 0.3f;
        [SerializeField] private float _departDuration = 0.25f;
        [SerializeField] private float _slotAppearDelay = 0.06f; // stagger entre piezas

        // ── State ─────────────────────────────────────────────
        private Vector3 _presentedPosition;
        private Vector3 _offscreenRight;
        private Vector3 _offscreenLeft;
        private List<GameObject> _activeSlots = new List<GameObject>();

        // ── Unity lifecycle ───────────────────────────────────
        private void Awake()
        {
            _presentedPosition = _customerSprite.transform.localPosition;
            _offscreenRight    = _presentedPosition + Vector3.right * 800f;
            _offscreenLeft     = _presentedPosition + Vector3.left  * 800f;
            HideCustomer();
        }

        // ── Public API ────────────────────────────────────────

        public void PresentCustomer(CustomerData data)
        {
            // Sprite del cliente
            Sprite sp = _visualConfig.GetSprite(data.SpriteKey);
            if (sp != null) _customerSprite.sprite = sp;

            // Limpiar slots anteriores
            ClearMoneySlots();

            // Mostrar piezas físicas con stagger
            StartCoroutine(SpawnMoneySlots(data.PhysicalMoney, clearFirst: false));

            // Slide in
            _customerSprite.transform.localPosition = _offscreenRight;
            _customerSprite.gameObject.SetActive(true);
            _billsContainer.gameObject.SetActive(true);

            StartCoroutine(SlideToPosition(_customerSprite.transform, _presentedPosition, _slideDuration));
        }

        /// <summary>Agrega las piezas extra al layout existente cuando el jugador pide más.</summary>
        public void ShowExtraMoney(CustomerData data)
        {
            if (data.ExtraPhysicalMoney == null || data.ExtraPhysicalMoney.Count == 0) return;
            StartCoroutine(SpawnMoneySlots(data.ExtraPhysicalMoney, clearFirst: false));
        }

        public void ShowArrivalAnimation() { }

        public void DepartCustomer(DecisionType decision)
        {
            Vector3 target = decision == DecisionType.Accept ? _offscreenLeft : _offscreenRight;
            StartCoroutine(SlideAndHide(_customerSprite.transform, target, _departDuration));
        }

        public void HideCustomer()
        {
            _customerSprite.gameObject.SetActive(false);
            _billsContainer.gameObject.SetActive(false);
            ClearMoneySlots();
        }

        // ── Spawn de slots ────────────────────────────────────

        private IEnumerator SpawnMoneySlots(List<DenominationInstance> pieces, bool clearFirst)
        {
            if (clearFirst) ClearMoneySlots();
            if (pieces == null) yield break;

            // Separar billetes de monedas para layout en dos filas
            var bills = new List<DenominationInstance>();
            var coins = new List<DenominationInstance>();

            foreach (var p in pieces)
            {
                if (p.Kind == DenominationKind.Bill) bills.Add(p);
                else                                  coins.Add(p);
            }

            // Calcular offset de inicio para centrar cada fila
            int existingBills = CountActiveByKind(DenominationKind.Bill);
            int existingCoins = CountActiveByKind(DenominationKind.Coin);

            // Spawnear billetes en fila superior
            foreach (var piece in bills)
            {
                SpawnSlot(piece, existingBills, rowOffsetY: 0f);
                existingBills++;
                yield return new WaitForSeconds(_slotAppearDelay);
            }

            // Spawnear monedas en fila inferior
            foreach (var piece in coins)
            {
                SpawnSlot(piece, existingCoins, rowOffsetY: _coinRowOffsetY);
                existingCoins++;
                yield return new WaitForSeconds(_slotAppearDelay);
            }
        }

        private void SpawnSlot(DenominationInstance piece, int indexInRow, float rowOffsetY)
        {
            if (_moneySlotPrefab == null) return;

            GameObject slot = Instantiate(_moneySlotPrefab, _billsContainer);
            _activeSlots.Add(slot);

            // Posición en la fila
            float x = (indexInRow - 0f) * _slotSpacing;
            slot.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, rowOffsetY);

            // Asignar sprite
            Image img = slot.GetComponent<Image>();
            if (img != null && _moneyVisualConfig != null)
            {
                Sprite sp = _moneyVisualConfig.GetSprite(piece.SpriteKey);
                if (sp != null) img.sprite = sp;
            }

            // Animación de aparición (scale pop)
            slot.transform.localScale = Vector3.zero;
            StartCoroutine(PopIn(slot.transform));
        }

        private void ClearMoneySlots()
        {
            foreach (var slot in _activeSlots)
                if (slot != null) Destroy(slot);
            _activeSlots.Clear();
        }

        private int CountActiveByKind(DenominationKind kind)
        {
            // No tenemos referencia directa al kind en los slots instanciados,
            // así que lo inferimos por la posición Y del slot
            int count = 0;
            foreach (var slot in _activeSlots)
            {
                if (slot == null) continue;
                float y = slot.GetComponent<RectTransform>().anchoredPosition.y;
                bool isCoinRow = y < -10f;
                if (kind == DenominationKind.Coin && isCoinRow)  count++;
                if (kind == DenominationKind.Bill && !isCoinRow) count++;
            }
            return count;
        }

        // ── Animaciones ───────────────────────────────────────

        private IEnumerator PopIn(Transform t)
        {
            float elapsed  = 0f;
            float duration = 0.15f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float s = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                t.localScale = Vector3.one * s;
                yield return null;
            }
            t.localScale = Vector3.one;
        }

        private IEnumerator SlideToPosition(Transform t, Vector3 target, float duration)
        {
            Vector3 start = t.localPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                t.localPosition = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, elapsed / duration));
                yield return null;
            }
            t.localPosition = target;
        }

        private IEnumerator SlideAndHide(Transform t, Vector3 target, float duration)
        {
            yield return SlideToPosition(t, target, duration);
            HideCustomer();
        }
    }
}
