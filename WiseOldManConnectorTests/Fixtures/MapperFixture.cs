using AutoMapper;

namespace WiseOldManConnectorTests.Fixtures {
    public class MapperFixture {

        public Mapper Mapper { get; set; }

        public MapperFixture() {
            Mapper = WiseOldManConnector.Transformers.Configuration.GetMapper();
        }
    }
}