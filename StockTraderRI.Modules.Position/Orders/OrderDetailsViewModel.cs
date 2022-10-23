using System;
using System.Collections.Generic;
using System.Globalization;
using Prism.Commands;
using Prism.Mvvm;
using StockTraderRI.Infrastructure;
using StockTraderRI.Infrastructure.Interfaces;
using StockTraderRI.Infrastructure.Models;
using StockTraderRI.Modules.Position.Interfaces;
using StockTraderRI.Modules.Position.Models;


namespace StockTraderRI.Modules.Position.Orders
{
    public class OrderDetailsViewModel : BindableBase, IOrderDetailsViewModel
    {
        private readonly IAccountPositionService accountPositionService;
        private readonly List<string> errors = new List<string>();
        private readonly IOrdersService ordersService;
        private OrderType orderType = OrderType.Market;
        private int? shares;
        private decimal? stopLimitPrice;
        private TimeInForce timeInForce;
        private TransactionInfo transactionInfo;

        public OrderDetailsViewModel(IAccountPositionService accountPositionService, IOrdersService ordersService)
        {
            this.accountPositionService = accountPositionService;
            this.ordersService = ordersService;

            this.transactionInfo = new TransactionInfo();
            var resourcesOrderType_Limit = "Limit";
            var resourcesOrderType_Market = "Market";
            var resourcesOrderType_Stop = "Stop";
            var resourcesTimeInForce_EndOfDay = "End of day";
            var resourcesTimeInForceThirtyDays = "Thirty days";
            //use localizable enum descriptions
            this.AvailableOrderTypes = new ValueDescriptionList<OrderType>
                                        {
                                            new ValueDescription<OrderType>(OrderType.Limit, resourcesOrderType_Limit),
                                            new ValueDescription<OrderType>(OrderType.Market, resourcesOrderType_Market),
                                            new ValueDescription<OrderType>(OrderType.Stop, resourcesOrderType_Stop)
                                        };

            this.AvailableTimesInForce = new ValueDescriptionList<TimeInForce>
                                          {
                                              new ValueDescription<TimeInForce>(TimeInForce.EndOfDay, resourcesTimeInForce_EndOfDay),
                                              new ValueDescription<TimeInForce>(TimeInForce.ThirtyDays, resourcesTimeInForceThirtyDays)
                                          };

            this.SubmitCommand = new DelegateCommand<object>(this.Submit, this.CanSubmit);
            this.CancelCommand = new DelegateCommand<object>(this.Cancel);

            this.SetInitialValidState();
        }

        public event EventHandler CloseViewRequested = delegate { };

        public IValueDescriptionList<OrderType> AvailableOrderTypes { get; private set; }

        public IValueDescriptionList<TimeInForce> AvailableTimesInForce { get; private set; }

        public DelegateCommand<object> CancelCommand { get; private set; }

        public OrderType OrderType
        {
            get { return this.orderType; }
            set
            {
                SetProperty(ref this.orderType, value);
            }
        }

        public int? Shares
        {
            get { return this.shares; }
            set
            {
                this.ValidateShares(value, true);
                this.ValidateHasEnoughSharesToSell(value, this.TransactionType, true);

                SetProperty(ref this.shares, value);
            }
        }

        public decimal? StopLimitPrice
        {
            get
            {
                return this.stopLimitPrice;
            }
            set
            {
                this.ValidateStopLimitPrice(value, true);

                SetProperty(ref this.stopLimitPrice, value);
            }
        }

        public DelegateCommand<object> SubmitCommand { get; private set; }

        public string TickerSymbol
        {
            get { return this.transactionInfo.TickerSymbol; }
            set
            {
                if (this.transactionInfo.TickerSymbol != value)
                {
                    this.transactionInfo.TickerSymbol = value;
                }
            }
        }

        public TimeInForce TimeInForce
        {
            get { return this.timeInForce; }
            set
            {
                SetProperty(ref this.timeInForce, value);
            }
        }

        public TransactionInfo TransactionInfo
        {
            get { return this.transactionInfo; }
            set
            {
                SetProperty(ref this.transactionInfo, value);
            }
        }

        public TransactionType TransactionType
        {
            get { return this.transactionInfo.TransactionType; }
            set
            {
                this.ValidateHasEnoughSharesToSell(this.Shares, value, false);
                if (this.transactionInfo.TransactionType != value)
                {
                    this.transactionInfo.TransactionType = value;
                    RaisePropertyChanged();
                }
            }
        }

        private void AddError(string ruleName)
        {
            if (!this.errors.Contains(ruleName))
            {
                this.errors.Add(ruleName);
                this.SubmitCommand.RaiseCanExecuteChanged();
            }
        }

        private void Cancel(object parameter)
        {
            CloseViewRequested(this, EventArgs.Empty);
        }

        private bool CanSubmit(object parameter)
        {
            return this.errors.Count == 0;
        }

        private bool HoldsEnoughShares(string symbol, int? sharesToSell)
        {
            if (!sharesToSell.HasValue)
            {
                return false;
            }

            foreach (AccountPosition accountPosition in this.accountPositionService.GetAccountPositions())
            {
                if (accountPosition.TickerSymbol.Equals(symbol, StringComparison.OrdinalIgnoreCase))
                {
                    if (accountPosition.Shares >= sharesToSell)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        private void RemoveError(string ruleName)
        {
            if (this.errors.Contains(ruleName))
            {
                this.errors.Remove(ruleName);
                if (this.errors.Count == 0)
                {
                    this.SubmitCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private void SetInitialValidState()
        {
            this.ValidateShares(this.Shares, false);
            this.ValidateStopLimitPrice(this.StopLimitPrice, false);
        }

        private void Submit(object parameter)
        {
            if (!this.CanSubmit(parameter))
            {
                throw new InvalidOperationException();
            }

            var order = new Order();
            order.TransactionType = this.TransactionType;
            order.OrderType = this.OrderType;
            order.Shares = this.Shares.Value;
            order.StopLimitPrice = this.StopLimitPrice.Value;
            order.TickerSymbol = this.TickerSymbol;
            order.TimeInForce = this.TimeInForce;

            ordersService.Submit(order);

            CloseViewRequested(this, EventArgs.Empty);
        }

        private void ValidateHasEnoughSharesToSell(int? sharesToSell, TransactionType transactionType, bool throwException)
        {
            if (transactionType == TransactionType.Sell && !this.HoldsEnoughShares(this.TickerSymbol, sharesToSell))
            {
                this.AddError("NotEnoughSharesToSell");
                if (throwException)
                {
                    var resourcesNotEnoughSharesToSell = "You don't have {0} shares to sell";
                    throw new InputValidationException(String.Format(CultureInfo.InvariantCulture, resourcesNotEnoughSharesToSell, sharesToSell));
                }
            }
            else
            {
                this.RemoveError("NotEnoughSharesToSell");
            }
        }

        private void ValidateShares(int? newSharesValue, bool throwException)
        {
            if (!newSharesValue.HasValue || newSharesValue.Value <= 0)
            {
                this.AddError("InvalidSharesRange");
                if (throwException)
                {
                    var resourcesInvalidSharesRange = "Shares must be greater than 0";
                    throw new InputValidationException(resourcesInvalidSharesRange);
                }
            }
            else
            {
                this.RemoveError("InvalidSharesRange");
            }
        }

        private void ValidateStopLimitPrice(decimal? price, bool throwException)
        {
            if (!price.HasValue || price.Value <= 0)
            {
                this.AddError("InvalidStopLimitPrice");
                if (throwException)
                {
                    var resourcesInvalidStopLimitPrice= "The stop limit price must be greater than 0";
                    throw new InputValidationException(resourcesInvalidStopLimitPrice);
                }
            }
            else
            {
                this.RemoveError("InvalidStopLimitPrice");
            }
        }
    }
}