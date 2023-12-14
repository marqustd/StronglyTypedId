namespace StronglyTypedIds.Sample.Tests;

public class SampleRecordTests
{
    [Fact]
    public void SampleRecord_Value()
    {
        var sut = new SampleRecord("string");
        Assert.Equal("STRING", sut.Value);
    }
}