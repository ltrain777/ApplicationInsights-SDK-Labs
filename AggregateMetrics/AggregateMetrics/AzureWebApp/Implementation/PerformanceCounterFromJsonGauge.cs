﻿namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;

    /// <summary>
    /// Gauge that gives the user an aggregate of requested counters in a cache
    /// </summary>
    internal class PerformanceCounterFromJsonGauge : ICounterValue
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

        private ICachedEnvironmentVariableAccess cacheHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceCounterFromJsonGauge"/> class.
        /// </summary>
        /// <param name="name">Name of counter variable.</param>
        /// <param name="jsonId">Json identifier of the counter variable.</param>
        /// <param name="environmentVariable">Identifier of the environment variable.</param>
        public PerformanceCounterFromJsonGauge(string name, string jsonId, AzureWebApEnvironmentVariables environmentVariable)
            : this(name, jsonId, environmentVariable, CacheHelper.Instance)
        {
        }

        internal PerformanceCounterFromJsonGauge(string name, string jsonId, AzureWebApEnvironmentVariables environmentVariable, ICachedEnvironmentVariableAccess cache)
        {
            this.name = name;
            this.jsonId = jsonId;
            this.environmentVariable = environmentVariable;
            this.cacheHelper = cache;
        }

        /// <summary>
        /// Returns the current value of the counter as a <c ref="MetricTelemetry"/> and resets the metric.
        /// </summary>
        /// <returns> Metric Telemetry object, with values for Name and Value </returns>
        public MetricTelemetry GetValueAndReset()
        {
            var metric = new MetricTelemetry();

            metric.Name = this.name;
            metric.Value = this.cacheHelper.GetCounterValue(this.jsonId, this.environmentVariable);

            return metric;
        }
    }
}
