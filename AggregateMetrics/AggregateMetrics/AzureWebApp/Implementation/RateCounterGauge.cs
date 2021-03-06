﻿namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    using System;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;
    using Microsoft.ApplicationInsights.Extensibility.Implementation;

    /// <summary>
    /// Struct for metrics dependant on time.
    /// </summary>
    internal class RateCounterGauge : ICounterValue
    {
        /// <summary>
        /// Name of the counter.
        /// </summary>
        private string name;

        /// <summary>
        /// Json identifier of the counter variable.
        /// </summary>
        private string jsonId;

        /// <summary>
        /// Identifier of the environment variable.
        /// </summary>
        private AzureWebApEnvironmentVariables environmentVariable;

        private ICounterValue counter;

        private double? lastValue;

        private ICachedEnvironmentVariableAccess cacheHelper;

        /// <summary>
        /// DateTime object to keep track of the last time this metric was retrieved.
        /// </summary>
        private DateTimeOffset dateTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCounterGauge"/> class.
        /// </summary>
        /// <param name="name"> Name of counter variable.</param>
        /// <param name="jsonId">Json identifier of the counter variable.</param>
        /// <param name="environmentVariable"> Identifier of the corresponding environment variable.</param>
        /// <param name="counter">Dependant counter.</param>
        public RateCounterGauge(string name, string jsonId, AzureWebApEnvironmentVariables environmentVariable, ICounterValue counter = null)
            : this(name, jsonId, environmentVariable, counter, CacheHelper.Instance)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="RateCounterGauge"/> class. 
        /// This constructor is intended for Unit Tests.
        /// </summary>
        /// <param name="name"> Name of the counter variable.</param>
        /// <param name="jsonId">Json identifier of the counter variable.</param>
        /// <param name="environmentVariable"> Identifier of the corresponding environment variable.</param>
        /// <param name="counter">Dependant counter.</param>
        /// <param name="cache"> Cache object.</param>
        internal RateCounterGauge(string name, string jsonId, AzureWebApEnvironmentVariables environmentVariable, ICounterValue counter, ICachedEnvironmentVariableAccess cache)
        {
            this.name = name;
            this.jsonId = jsonId;
            this.environmentVariable = environmentVariable;
            this.counter = counter;
            this.cacheHelper = cache;
        }

        /// <summary>
        /// Returns the current value of the rate counter if enough information exists.
        /// </summary>
        /// <returns> MetricTelemetry object.</returns>
        public MetricTelemetry GetValueAndReset()
        {
            MetricTelemetry metric = new MetricTelemetry();
            DateTimeOffset currentTime = System.DateTimeOffset.Now;

            var timeDifferenceInSeconds = currentTime.Subtract(this.dateTime).Seconds;
            metric.Name = this.name;
            metric.Context.GetInternalContext().SdkVersion = SdkVersionAzureWebApp.sdkVersionAzureWebApp;

            if (this.lastValue == null)
            {
                if (this.counter == null)
                {
                    this.lastValue = this.cacheHelper.GetCounterValue(this.jsonId, this.environmentVariable);
                }
                else
                {
                    this.lastValue = this.counter.GetValueAndReset().Value;
                }

                this.dateTime = currentTime;

                return metric;
            }

            if (this.counter == null)
            {
                metric.Value = (timeDifferenceInSeconds != 0) ? (this.cacheHelper.GetCounterValue(this.jsonId, this.environmentVariable) - (double)this.lastValue) / timeDifferenceInSeconds : 0;
                this.lastValue = this.cacheHelper.GetCounterValue(this.jsonId, this.environmentVariable);
            }
            else
            {
                metric.Value = (timeDifferenceInSeconds != 0) ? (this.counter.GetValueAndReset().Value - (double)this.lastValue) / timeDifferenceInSeconds : 0;
                this.lastValue = this.counter.GetValueAndReset().Value;
            }

            this.dateTime = currentTime;

            return metric;
        }
    }
}
