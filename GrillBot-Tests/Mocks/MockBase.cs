using Moq;

namespace GrillBot_Tests.Mocks
{
    public abstract class MockBase<T> where T : class
    {
        private Mock<T> Mock { get; }

        protected MockBase(object[] dependencies)
        {
            Mock = dependencies?.Length > 0 ? new Mock<T>(dependencies) : new Mock<T>();
        }

        protected Mock<T> GetMock() => Mock;

        public T GetObject() => Mock.Object;
    }
}
