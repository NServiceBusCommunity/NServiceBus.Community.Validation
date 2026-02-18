public class TestHelperTests
{
    [Test]
    public Task FindHandledMessagesWithoutValidator()
    {
        var types = TestHelper.FindHandledMessagesWithoutValidator(typeof(Handler).Assembly, false);
        return Verify(types.ToList());
    }

    [Test]
    public Task FindMessagesWithoutValidator()
    {
        var types = TestHelper.FindMessagesWithoutValidator(typeof(Handler).Assembly, false);
        return Verify(types.ToList());
    }
}