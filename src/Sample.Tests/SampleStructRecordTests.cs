namespace Sample.Tests;

public class SampleStructRecordTests
{
    [Fact]
    public void SampleStructRecord_Value()
    {
        var sut = new SampleStructRecord("string");
        Assert.Equal("string", sut.Value);
    }
    
    [Fact]
    public void UpperStructRecord_Value()
    {
        var sut = new UpperStructRecord("string");
        Assert.Equal("STRING", sut.Value);
    }
    
    [Fact]
    public void ValidatedStructRecordThrow()
    {
        Assert.Throws<ArgumentException>(() => ValidatedStructRecord.Parse("four"));
    }
    
    [Fact]
    public void ValidatedStructRecord_Ok()
    {
        ValidatedStructRecord.Parse("fourAndFive");
        Assert.True(ValidatedStructRecord.TryParse("fourAndFive", out _));
    }

}