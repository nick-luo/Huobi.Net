﻿using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using Huobi.Net.Converters;
using Huobi.Net.Interfaces;
using Huobi.Net.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Huobi.Net
{
    public class HuobiClient: RestClient, IHuobiClient
    {
        #region fields
        private static HuobiClientOptions defaultOptions = new HuobiClientOptions();
        private static HuobiClientOptions DefaultOptions => defaultOptions.Copy<HuobiClientOptions>();


        private const string MarketTickerEndpoint = "market/tickers";
        private const string MarketTickerMergedEndpoint = "market/detail/merged";
        private const string MarketKlineEndpoint = "market/history/kline";
        private const string MarketDepthEndpoint = "market/depth";
        private const string MarketLastTradeEndpoint = "market/trade";
        private const string MarketTradeHistoryEndpoint = "market/history/trade";
        private const string MarketDetailsEndpoint = "market/detail";

        private const string CommonSymbolsEndpoint = "common/symbols";
        private const string CommonCurrenciesEndpoint = "common/currencys";
        private const string ServerTimeEndpoint = "common/timestamp";

        private const string GetAccountsEndpoint = "account/accounts";
        private const string GetBalancesEndpoint = "account/accounts/{}/balance";

        private const string PlaceOrderEndpoint = "order/orders/place";
        private const string OpenOrdersEndpoint = "order/openOrders";
        private const string OrdersEndpoint = "order/orders";
        private const string CancelOrderEndpoint = "order/orders/{}/submitcancel";
        private const string CancelOrdersEndpoint = "order/orders/batchcancel";
        private const string OrderInfoEndpoint = "order/orders/{}";
        private const string OrderTradesEndpoint = "order/orders/{}/matchresults";
        private const string SymbolTradesEndpoint = "order/matchresults";

        #endregion

        #region constructor/destructor
        /// <summary>
        /// Create a new instance of HuobiClient using the default options
        /// </summary>
        public HuobiClient() : this(DefaultOptions)
        {
        }

        /// <summary>
        /// Create a new instance of the HuobiClient with the provided options
        /// </summary>
        public HuobiClient(HuobiClientOptions options) : base(options, options.ApiCredentials == null ? null : new HuobiAuthenticationProvider(options.ApiCredentials))
        {
            Configure(options);
        }
        #endregion

        #region methods
        /// <summary>
        /// Sets the default options to use for new clients
        /// </summary>
        /// <param name="options">The options to use for new clients</param>
        public static void SetDefaultOptions(HuobiClientOptions options)
        {
            defaultOptions = options;
        }

        /// <summary>
        /// Set the API key and secret
        /// </summary>
        /// <param name="apiKey">The api key</param>
        /// <param name="apiSecret">The api secret</param>
        public void SetApiCredentials(string apiKey, string apiSecret)
        {
            SetAuthenticationProvider(new HuobiAuthenticationProvider(new ApiCredentials(apiKey, apiSecret)));
        }

        /// <summary>
        /// Gets the latest ticker for all markets
        /// </summary>
        /// <returns></returns>
        public CallResult<HuobiTimestampResponse<List<HuobiMarketTick>>> GetMarketTickers() => GetMarketTickersAsync().Result;
        /// <summary>
        /// Gets the latest ticker for all markets
        /// </summary>
        /// <returns></returns>
        public async Task<CallResult<HuobiTimestampResponse<List<HuobiMarketTick>>>> GetMarketTickersAsync()
        {
            return GetResult(await ExecuteRequest<HuobiTimestampResponse<List<HuobiMarketTick>>>(GetUrl(MarketTickerEndpoint)));            
        }

        /// <summary>
        /// Gets the ticker, including the best bid / best ask for a symbol
        /// </summary>
        /// <param name="symbol">The symbol to get the ticker for</param>
        /// <returns></returns>
        public CallResult<HuobiChannelResponse<HuobiMarketTickMerged>> GetMarketTickerMerged(string symbol) => GetMarketTickerMergedAsync(symbol).Result;

        /// <summary>
        /// Gets the ticker, including the best bid / best ask for a symbol
        /// </summary>
        /// <param name="symbol">The symbol to get the ticker for</param>
        /// <returns></returns>
        public async Task<CallResult<HuobiChannelResponse<HuobiMarketTickMerged>>> GetMarketTickerMergedAsync(string symbol)
        {
            var parameters = new Dictionary<string, object>
            {
                { "symbol", symbol }
            };

            return GetResult(await ExecuteRequest<HuobiChannelResponse<HuobiMarketTickMerged>>(GetUrl(MarketTickerMergedEndpoint), parameters: parameters, checkResult:false));
        }

        /// <summary>
        /// Get candlestick data for a market
        /// </summary>
        /// <param name="symbol">The symbol to get the data for</param>
        /// <param name="period">The period of a single candlestick</param>
        /// <param name="size">The amount of candlesticks</param>
        /// <returns></returns>
        public CallResult<HuobiChannelResponse<List<HuobiMarketData>>> GetMarketKlines(string symbol, HuobiPeriod period, int size) => GetMarketKlinesAsync(symbol, period, size).Result;

        /// <summary>
        /// Get candlestick data for a market
        /// </summary>
        /// <param name="symbol">The symbol to get the data for</param>
        /// <param name="period">The period of a single candlestick</param>
        /// <param name="size">The amount of candlesticks</param>
        /// <returns></returns>
        public async Task<CallResult<HuobiChannelResponse<List<HuobiMarketData>>>> GetMarketKlinesAsync(string symbol, HuobiPeriod period, int size)
        {
            if (size <= 0 || size > 2000)
                return new CallResult<HuobiChannelResponse<List<HuobiMarketData>>>(null, new ArgumentError("Size should be between 1 and 2000"));

            var parameters = new Dictionary<string, object>
            {
                { "symbol", symbol },
                { "period", JsonConvert.SerializeObject(period, new PeriodConverter(false)) },
                { "size", size },
            };

            return GetResult(await ExecuteRequest<HuobiChannelResponse<List<HuobiMarketData>>>(GetUrl(MarketKlineEndpoint), parameters: parameters));
        }

        /// <summary>
        /// Gets the market depth for a symbol
        /// </summary>
        /// <param name="symbol">The symbol to request for</param>
        /// <param name="mergeStep">The way the results will be merged together</param>
        /// <returns></returns>
        public CallResult<HuobiChannelResponse<HuobiMarketDepth>> GetMarketDepth(string symbol, int mergeStep) => GetMarketDepthAsync(symbol, mergeStep).Result;
        /// <summary>
        /// Gets the market depth for a symbol
        /// </summary>
        /// <param name="symbol">The symbol to request for</param>
        /// <param name="mergeStep">The way the results will be merged together</param>
        /// <returns></returns>
        public async Task<CallResult<HuobiChannelResponse<HuobiMarketDepth>>> GetMarketDepthAsync(string symbol, int mergeStep)
        {
            if (mergeStep < 0 || mergeStep > 5)
                return new CallResult<HuobiChannelResponse<HuobiMarketDepth>>(null, new ArgumentError("MergeStep should be between 0 and 5"));

            var parameters = new Dictionary<string, object>
            {
                { "symbol", symbol },
                { "type", "step"+mergeStep },
            };

            return GetResult(await ExecuteRequest<HuobiChannelResponse<HuobiMarketDepth>>(GetUrl(MarketDepthEndpoint), parameters: parameters, checkResult: false));
        }

        /// <summary>
        /// Gets the last trade for a market
        /// </summary>
        /// <param name="symbol">The symbol to request for</param>
        /// <returns></returns>
        public CallResult<HuobiChannelResponse<HuobiMarketTrade>> GetMarketLastTrade(string symbol) => GetMarketLastTradeAsync(symbol).Result;
        /// <summary>
        /// Gets the last trade for a market
        /// </summary>
        /// <param name="symbol">The symbol to request for</param>
        /// <returns></returns>
        public async Task<CallResult<HuobiChannelResponse<HuobiMarketTrade>>> GetMarketLastTradeAsync(string symbol)
        {
            var parameters = new Dictionary<string, object>
            {
                { "symbol", symbol }
            };

            return GetResult(await ExecuteRequest<HuobiChannelResponse<HuobiMarketTrade>>(GetUrl(MarketLastTradeEndpoint), parameters: parameters, checkResult: false));
        }

        /// <summary>
        /// Get the last x trades for a market
        /// </summary>
        /// <param name="symbol">The market to get trades for</param>
        /// <param name="limit">The max number of results</param>
        /// <returns></returns>
        public CallResult<HuobiChannelResponse<List<HuobiMarketTrade>>> GetMarketTradeHistory(string symbol, int limit) => GetMarketTradeHistoryAsync(symbol, limit).Result;
        /// <summary>
        /// Get the last x trades for a market
        /// </summary>
        /// <param name="symbol">The market to get trades for</param>
        /// <param name="limit">The max number of results</param>
        /// <returns></returns>
        public async Task<CallResult<HuobiChannelResponse<List<HuobiMarketTrade>>>> GetMarketTradeHistoryAsync(string symbol, int limit)
        {
            if (limit <= 0 || limit > 2000)
                return new CallResult<HuobiChannelResponse<List<HuobiMarketTrade>>>(null, new ArgumentError("Size should be between 1 and 2000"));

            var parameters = new Dictionary<string, object>
            {
                { "symbol", symbol },
                { "size", limit },
            };

            return GetResult(await ExecuteRequest<HuobiChannelResponse<List<HuobiMarketTrade>>>(GetUrl(MarketTradeHistoryEndpoint), parameters: parameters));
        }

        /// <summary>
        /// Gets 24h stats for a market
        /// </summary>
        /// <param name="symbol">The market to get the data for</param>
        /// <returns></returns>
        public CallResult<HuobiChannelResponse<HuobiMarketData>> GetMarketDetails24H(string symbol) => GetMarketDetails24HAsync(symbol).Result;
        /// <summary>
        /// Gets 24h stats for a market
        /// </summary>
        /// <param name="symbol">The market to get the data for</param>
        /// <returns></returns>
        public async Task<CallResult<HuobiChannelResponse<HuobiMarketData>>> GetMarketDetails24HAsync(string symbol)
        {
            var parameters = new Dictionary<string, object>
            {
                { "symbol", symbol }
            };

            return GetResult(await ExecuteRequest<HuobiChannelResponse<HuobiMarketData>>(GetUrl(MarketDetailsEndpoint), parameters: parameters, checkResult: false));
        }

        /// <summary>
        /// Gets a list of supported symbols
        /// </summary>
        /// <returns></returns>
        public CallResult<List<HuobiSymbol>> GetSymbols() => GetSymbolsAsync().Result;
        /// <summary>
        /// Gets a list of supported symbols
        /// </summary>
        /// <returns></returns>
        public async Task<CallResult<List<HuobiSymbol>>> GetSymbolsAsync()
        {
            var result = GetResult(await ExecuteRequest<HuobiBasicResponse<List<HuobiSymbol>>>(GetUrl(CommonSymbolsEndpoint, "1")));
            return new CallResult<List<HuobiSymbol>>(result.Data?.Data, result.Error);
        }

        /// <summary>
        /// Gets a list of supported currencies
        /// </summary>
        /// <returns></returns>
        public CallResult<List<string>> GetCurrencies() => GetCurrenciesAsync().Result;
        /// <summary>
        /// Gets a list of supported currencies
        /// </summary>
        /// <returns></returns>
        public async Task<CallResult<List<string>>> GetCurrenciesAsync()
        {
            var result = GetResult(await ExecuteRequest<HuobiBasicResponse<List<string>>>(GetUrl(CommonCurrenciesEndpoint, "1")));
            return new CallResult<List<string>>(result.Data?.Data, result.Error);
        }

        /// <summary>
        /// Gets the server time
        /// </summary>
        /// <returns></returns>
        public CallResult<DateTime> GetServerTime() => GetServerTimeAsync().Result;
        /// <summary>
        /// Gets the server time
        /// </summary>
        /// <returns></returns>
        public async Task<CallResult<DateTime>> GetServerTimeAsync()
        {
            var result = GetResult(await ExecuteRequest<HuobiBasicResponse<string>>(GetUrl(ServerTimeEndpoint, "1")));
            if (!result.Success)
                return new CallResult<DateTime>(default(DateTime), result.Error);
            var time = (DateTime)JsonConvert.DeserializeObject(result.Data.Data, typeof(DateTime), new TimestampConverter());
            return new CallResult<DateTime>(time, null);
        }

        /// <summary>
        /// Gets a list of accounts associated with the apikey/secret
        /// </summary>
        /// <returns></returns>
        public CallResult<List<HuobiAccount>> GetAccounts() => GetAccountsAsync().Result;
        /// <summary>
        /// Gets a list of accounts associated with the apikey/secret
        /// </summary>
        /// <returns></returns>
        public async Task<CallResult<List<HuobiAccount>>> GetAccountsAsync()
        {
            var result = GetResult(await ExecuteRequest<HuobiBasicResponse<List<HuobiAccount>>>(GetUrl(GetAccountsEndpoint, "1"), signed: true));
            return new CallResult<List<HuobiAccount>>(result.Data?.Data, result.Error);
        }

        /// <summary>
        /// Gets a list of balances for a specific account
        /// </summary>
        /// <param name="accountId">The id of the account to get the balances for</param>
        /// <returns></returns>
        public CallResult<HuobiAccountBalances> GetBalances(long accountId) => GetBalancesAsync(accountId).Result;
        /// <summary>
        /// Gets a list of balances for a specific account
        /// </summary>
        /// <param name="accountId">The id of the account to get the balances for</param>
        /// <returns></returns>
        public async Task<CallResult<HuobiAccountBalances>> GetBalancesAsync(long accountId)
        {
            var result = GetResult(await ExecuteRequest<HuobiBasicResponse<HuobiAccountBalances>>(GetUrl(FillPathParameter(GetBalancesEndpoint, accountId.ToString()), "1"), signed: true));
            return new CallResult<HuobiAccountBalances>(result.Data?.Data, result.Error);
        }

        /// <summary>
        /// Places an order
        /// </summary>
        /// <param name="accountId">The account to place the order for</param>
        /// <param name="symbol">The symbol to place the order for</param>
        /// <param name="orderType">The type of the order</param>
        /// <param name="amount">The amount of the order</param>
        /// <param name="price">The price of the order. Should be omitted for market orders</param>
        /// <returns></returns>
        public CallResult<long> PlaceOrder(long accountId, string symbol, HuobiOrderType orderType, decimal amount, decimal? price = null) => PlaceOrderAsync(accountId, symbol, orderType, amount, price).Result;
        /// <summary>
        /// Places an order
        /// </summary>
        /// <param name="accountId">The account to place the order for</param>
        /// <param name="symbol">The symbol to place the order for</param>
        /// <param name="orderType">The type of the order</param>
        /// <param name="amount">The amount of the order</param>
        /// <param name="price">The price of the order. Should be omitted for market orders</param>
        /// <returns></returns>
        public async Task<CallResult<long>> PlaceOrderAsync(long accountId, string symbol, HuobiOrderType orderType, decimal amount, decimal? price = null)
        {
            var parameters = new Dictionary<string, object>
            {
                { "account-id", accountId },
                { "amount", amount },
                { "symbol", symbol },
                { "type", JsonConvert.SerializeObject(orderType, new OrderTypeConverter(false)) }
            };

            parameters.AddOptionalParameter("price", price);
            var result = GetResult(await ExecuteRequest<HuobiBasicResponse<long>>(GetUrl(PlaceOrderEndpoint, "1"), "POST", parameters, true));
            return new CallResult<long>(result.Data?.Data ?? 0, result.Error);
        }

        /// <summary>
        /// Gets a list of open orders
        /// </summary>
        /// <param name="accountId">The account id for which to get the orders for</param>
        /// <param name="symbol">The symbol for which to get the orders for</param>
        /// <param name="side">Only get buy or sell orders</param>
        /// <param name="limit">The max number of results</param>
        /// <returns></returns>
        public CallResult<List<HuobiOrder>> GetOpenOrders(long? accountId = null, string symbol = null, HuobiOrderSide? side = null, int? limit = null) => GetOpenOrdersAsync(accountId, symbol, side, limit).Result;
        /// <summary>
        /// Gets a list of open orders
        /// </summary>
        /// <param name="accountId">The account id for which to get the orders for</param>
        /// <param name="symbol">The symbol for which to get the orders for</param>
        /// <param name="side">Only get buy or sell orders</param>
        /// <param name="limit">The max number of results</param>
        /// <returns></returns>
        public async Task<CallResult<List<HuobiOrder>>> GetOpenOrdersAsync(long? accountId = null, string symbol = null, HuobiOrderSide? side = null, int? limit = null)
        {
            if (accountId != null && symbol == null)
                return new CallResult<List<HuobiOrder>>(null, new ArgumentError("Can't request open orders based on only the account id"));

            var parameters = new Dictionary<string, object>();
            parameters.AddOptionalParameter("account-id", accountId);
            parameters.AddOptionalParameter("symbol", symbol);
            parameters.AddOptionalParameter("side", side == null ? null: JsonConvert.SerializeObject(side, new OrderSideConverter(false)));
            parameters.AddOptionalParameter("size", limit);

            var result = GetResult(await ExecuteRequest<HuobiBasicResponse<List<HuobiOrder>>>(GetUrl(OpenOrdersEndpoint, "1"), "GET", parameters, signed: true));
            return new CallResult<List<HuobiOrder>>(result.Data?.Data, result.Error);
        }

        /// <summary>
        /// Cancels an open order
        /// </summary>
        /// <param name="orderId">The id of the order to cancel</param>
        /// <returns></returns>
        public CallResult<long> CancelOrder(long orderId) => CancelOrderAsync(orderId).Result;
        /// <summary>
        /// Cancels an open order
        /// </summary>
        /// <param name="orderId">The id of the order to cancel</param>
        /// <returns></returns>
        public async Task<CallResult<long>> CancelOrderAsync(long orderId)
        {
            var result = GetResult(await ExecuteRequest<HuobiBasicResponse<long>>(GetUrl(FillPathParameter(CancelOrderEndpoint, orderId.ToString()), "1"), "POST", signed: true));
            return new CallResult<long>(result.Data?.Data ?? 0, result.Error);
        }

        /// <summary>
        /// Cancel multiple open orders
        /// </summary>
        /// <param name="orderIds">The ids of the orders to cancel</param>
        /// <returns></returns>
        public CallResult<HuobiBatchCancelResult> CancelOrders(long[] orderIds) => CancelOrdersAsync(orderIds).Result;
        /// <summary>
        /// Cancel multiple open orders
        /// </summary>
        /// <param name="orderIds">The ids of the orders to cancel</param>
        /// <returns></returns>
        public async Task<CallResult<HuobiBatchCancelResult>> CancelOrdersAsync(long[] orderIds)
        {
            var parameters = new Dictionary<string, object>
            {
                { "order-ids", orderIds.Select(s => s.ToString()) }
            };

            var result = GetResult(await ExecuteRequest<HuobiBasicResponse<HuobiBatchCancelResult>>(GetUrl(CancelOrdersEndpoint, "1"), "POST", parameters, true));
            return new CallResult<HuobiBatchCancelResult>(result.Data?.Data, result.Error);
        }

        /// <summary>
        /// Get details of an order
        /// </summary>
        /// <param name="orderId">The id of the order to retrieve</param>
        /// <returns></returns>
        public CallResult<HuobiOrder> GetOrderInfo(long orderId) => GetOrderInfoAsync(orderId).Result;
        /// <summary>
        /// Get details of an order
        /// </summary>
        /// <param name="orderId">The id of the order to retrieve</param>
        /// <returns></returns>
        public async Task<CallResult<HuobiOrder>> GetOrderInfoAsync(long orderId)
        {
            var result = GetResult(await ExecuteRequest<HuobiBasicResponse<HuobiOrder>>(GetUrl(FillPathParameter(OrderInfoEndpoint, orderId.ToString()), "1"), "GET", signed: true));
            return new CallResult<HuobiOrder>(result.Data?.Data, result.Error);
        }

        /// <summary>
        /// Gets a list of trades made for a specific order
        /// </summary>
        /// <param name="orderId">The id of the order to get trades for</param>
        /// <returns></returns>
        public CallResult<List<HuobiOrderTrade>> GetOrderTrades(long orderId) => GetOrderTradesAsync(orderId).Result;
        /// <summary>
        /// Gets a list of trades made for a specific order
        /// </summary>
        /// <param name="orderId">The id of the order to get trades for</param>
        /// <returns></returns>
        public async Task<CallResult<List<HuobiOrderTrade>>> GetOrderTradesAsync(long orderId)
        {
            var result = GetResult(await ExecuteRequest<HuobiBasicResponse<List<HuobiOrderTrade>>>(GetUrl(FillPathParameter(OrderTradesEndpoint, orderId.ToString()), "1"), "GET", signed: true));
            return new CallResult<List<HuobiOrderTrade>>(result.Data?.Data, result.Error);
        }

        /// <summary>
        /// Gets a list of orders
        /// </summary>
        /// <param name="symbol">The symbol to get orders for</param>
        /// <param name="states">The states of orders to return</param>
        /// <param name="types">The types of orders to return</param>
        /// <param name="startTime">Only get orders after this date</param>
        /// <param name="endTime">Only get orders before this date</param>
        /// <param name="fromId">Only get orders with id's higher than this</param>
        /// <param name="limit">The max number of results</param>
        /// <returns></returns>
        public CallResult<List<HuobiOrder>> GetOrders(string symbol, HuobiOrderState[] states, HuobiOrderType[] types = null, DateTime? startTime = null, DateTime? endTime = null, long? fromId = null, int? limit = null) => GetOrdersAsync(symbol, states, types, startTime, endTime, fromId, limit).Result;
        /// <summary>
        /// Gets a list of orders
        /// </summary>
        /// <param name="symbol">The symbol to get orders for</param>
        /// <param name="states">The states of orders to return</param>
        /// <param name="types">The types of orders to return</param>
        /// <param name="startTime">Only get orders after this date</param>
        /// <param name="endTime">Only get orders before this date</param>
        /// <param name="fromId">Only get orders with id's higher than this</param>
        /// <param name="limit">The max number of results</param>
        /// <returns></returns>
        public async Task<CallResult<List<HuobiOrder>>> GetOrdersAsync(string symbol, HuobiOrderState[] states, HuobiOrderType[] types = null, DateTime? startTime = null, DateTime? endTime = null, long? fromId = null, int? limit = null)
        {
            var stateConverter = new OrderStateConverter(false);
            var typeConverter = new OrderTypeConverter(false);
            var parameters = new Dictionary<string, object>()
            {
                { "symbol", symbol },
                { "states", string.Join(",", states.Select(s => JsonConvert.SerializeObject(s, stateConverter))) }
            };
            parameters.AddOptionalParameter("start-date", startTime?.ToString("yyyy-MM-dd"));
            parameters.AddOptionalParameter("end-date", endTime?.ToString("yyyy-MM-dd"));
            parameters.AddOptionalParameter("types", types == null ? null : string.Join(",", types.Select(s => JsonConvert.SerializeObject(s, typeConverter))));
            parameters.AddOptionalParameter("from", fromId);
            parameters.AddOptionalParameter("size", limit);

            var result = GetResult(await ExecuteRequest<HuobiBasicResponse<List<HuobiOrder>>>(GetUrl(OrdersEndpoint, "1"), "GET", parameters, signed: true));
            return new CallResult<List<HuobiOrder>>(result.Data?.Data, result.Error);
        }

        /// <summary>
        /// Gets a list of trades for a specific symbol
        /// </summary>
        /// <param name="symbol">The symbol to retrieve trades for</param>
        /// <param name="types">The type of orders to return</param>
        /// <param name="startTime">Only get orders after this date</param>
        /// <param name="endTime">Only get orders before this date</param>
        /// <param name="fromId">Only get orders with id's higher than this</param>
        /// <param name="limit">The max number of results</param>
        /// <returns></returns>
        public CallResult<List<HuobiOrderTrade>> GetSymbolTrades(string symbol, HuobiOrderType[] types = null, DateTime? startTime = null, DateTime? endTime = null, long? fromId = null, int? limit = null) => GetSymbolTradesAsync(symbol, types, startTime, endTime, fromId, limit).Result;
        /// <summary>
        /// Gets a list of trades for a specific symbol
        /// </summary>
        /// <param name="symbol">The symbol to retrieve trades for</param>
        /// <param name="types">The type of orders to return</param>
        /// <param name="startTime">Only get orders after this date</param>
        /// <param name="endTime">Only get orders before this date</param>
        /// <param name="fromId">Only get orders with id's higher than this</param>
        /// <param name="limit">The max number of results</param>
        /// <returns></returns>
        public async Task<CallResult<List<HuobiOrderTrade>>> GetSymbolTradesAsync(string symbol, HuobiOrderType[] types = null, DateTime? startTime = null, DateTime? endTime = null, long? fromId = null, int? limit = null)
        {
            var typeConverter = new OrderTypeConverter(false);
            var parameters = new Dictionary<string, object>()
            {
                { "symbol", symbol }
            };
            parameters.AddOptionalParameter("start-date", startTime?.ToString("yyyy-MM-dd"));
            parameters.AddOptionalParameter("end-date", endTime?.ToString("yyyy-MM-dd"));
            parameters.AddOptionalParameter("types", types == null ? null : string.Join(",", types.Select(s => JsonConvert.SerializeObject(s, typeConverter))));
            parameters.AddOptionalParameter("from", fromId);
            parameters.AddOptionalParameter("size", limit);

            var result = GetResult(await ExecuteRequest<HuobiBasicResponse<List<HuobiOrderTrade>>>(GetUrl(SymbolTradesEndpoint, "1"), "GET", parameters, signed: true));
            return new CallResult<List<HuobiOrderTrade>>(result.Data?.Data, result.Error);
        }

        protected override IRequest ConstructRequest(Uri uri, string method, Dictionary<string, object> parameters, bool signed)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            var uriString = uri.ToString();
            if (authProvider != null)
                parameters = authProvider.AddAuthenticationToParameters(uriString, method, parameters, signed);

            if ((method == Constants.GetMethod || method == Constants.DeleteMethod || (postParametersPosition == PostParameters.InUri)) && parameters?.Any() == true)
                uriString += "?" + parameters.CreateParamString(true);

            if (method == Constants.PostMethod && signed)
            {
                var uriParamNames = new[] { "AccessKeyId", "SignatureMethod", "SignatureVersion", "Timestamp", "Signature" };
                var uriParams = parameters.Where(p => uriParamNames.Contains(p.Key)).ToDictionary(k => k.Key, k => k.Value);
                uriString += "?" + uriParams.CreateParamString(true);
                parameters = parameters.Where(p => !uriParamNames.Contains(p.Key)).ToDictionary(k => k.Key, k => k.Value);
            }

            var request = RequestFactory.Create(uriString);
            request.ContentType = requestBodyFormat == RequestBodyFormat.Json ? Constants.JsonContentHeader : Constants.FormContentHeader;
            request.Accept = Constants.JsonContentHeader;
            request.Method = method;

            var headers = new Dictionary<string, string>();
            if (authProvider != null)
                headers = authProvider.AddAuthenticationToHeaders(uriString, method, parameters, signed);

            foreach (var header in headers)
                request.Headers.Add(header.Key, header.Value);

            if ((method == Constants.PostMethod || method == Constants.PutMethod) && postParametersPosition != PostParameters.InUri)
            {
                if (parameters?.Any() == true)
                    WriteParamBody(request, parameters);
                else
                    WriteParamBody(request, "{}");
            }

            return request;
        }

        protected override bool IsErrorResponse(JToken data)
        {
            return data["status"] != null && (string)data["status"] != "ok";
        }

        protected override Error ParseErrorResponse(JToken error)
        {
            if(error["err-code"] == null || error["err-msg"]==null)
                return new ServerError(error.ToString());

            return new ServerError($"{(string)error["err-code"]}, {(string)error["err-msg"]}");
        }


        private static CallResult<T> GetResult<T>(CallResult<T> result) where T: HuobiApiResponse
        {
            return new CallResult<T>(result.Success ? result.Data: null, result.Error);
        }        

        protected Uri GetUrl(string endpoint, string version=null)
        {
            if(version == null)
                return new Uri($"{BaseAddress}/{endpoint}");
            else
                return new Uri($"{BaseAddress}/v{version}/{endpoint}");
        }
        #endregion
    }
}
