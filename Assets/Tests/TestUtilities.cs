using NUnit.Framework;

namespace Tests
{
    public class TestUtilities
    {
        [Test]
        public void should_align_numbers()
        {
            Utilities.Align(0, 1);
        }
    }
}