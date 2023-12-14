namespace StronglyTypedIds.Sample;

[StronglyTypedId(StringTransformation.ToUpper, validator: typeof(ValidatedRecordValidator))]
public partial record ValidatedRecord;

internal class ValidatedRecordValidator : IValidator
{
    public bool Validate(string value)
    {
        return value.Length < 5;
    }
}