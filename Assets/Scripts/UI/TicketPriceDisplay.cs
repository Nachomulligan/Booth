// ============================================================
// TicketPriceDisplay.cs
// Shows the current ticket price in the UI.
// Simple display — reads from CustomerConfig on start.
// ============================================================

using UnityEngine;
using TMPro;
using Booth.Customers;

namespace Booth.UI
{
    public class TicketPriceDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private CustomerConfig  _customerConfig;

        private void Start()
        {
            if (_priceText != null && _customerConfig != null)
                _priceText.text = $"TICKET: ${_customerConfig.TicketPrice:F2}";
        }
    }
}
