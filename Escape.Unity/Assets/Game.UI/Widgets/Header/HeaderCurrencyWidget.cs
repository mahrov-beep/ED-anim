namespace Game.UI.Widgets.Header {
    using Controllers.Features.Store;
    using Domain.Currencies;
    using Multicast;
    using Shared;
    using UniMob.UI;
    using UnityEngine;
    using Views.Header;

    public class HeaderCurrencyWidget : StatefulWidget {
        public string CurrencyKey { get; }

        public HeaderCurrencyWidget(string currencyKey) {
            this.CurrencyKey = currencyKey;
        }
    }

    public class HeaderCurrencyState : ViewState<HeaderCurrencyWidget>, IHeaderCurrencyState {
        [Inject] private CurrenciesModel currenciesModel;

        public override WidgetViewReference View => UiConstants.Views.Header.HeaderCurrency;

        private CurrencyModel Currency =>  this.currenciesModel.Get(this.Widget.CurrencyKey);

        public string CurrencyKey => this.Currency.Key;
        public int    Amount      => this.Currency.Amount;

        public bool HasAddButton {
            get {
                if (this.Currency.Key == SharedConstants.Game.Currencies.CRYPT && 
                    (Application.isMobilePlatform || Application.isEditor)) {
                    return true;
                }

                return false;
            }
        }

        public void Add() {
            StoreFeatureEvents.Open.Raise();
        }
    }
}