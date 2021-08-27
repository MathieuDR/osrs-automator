using AutoMapper;

namespace WiseOldManConnectorTests.Fixtures {
    public class MapperFixture {
        public MapperFixture() {
            Mapper = WiseOldManConnector.Transformers.Configuration.GetMapper();
        }

        public Mapper Mapper { get; set; }
    }
}
