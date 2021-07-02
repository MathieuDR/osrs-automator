using System;
using System.Collections;
using System.Collections.Generic;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnectorTests.TransformerTests {
    public class MetricTypeTestData : IEnumerable<object[]> {
        public IEnumerator<object[]> GetEnumerator() {
            var metrics = Enum.GetValues(typeof(MetricType)) as MetricType[];

            foreach (MetricType metricType in metrics) {
                yield return new object[] {metricType};
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
