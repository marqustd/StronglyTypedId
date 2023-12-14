using StronglyTypedIds;

namespace Sample;

[StronglyTypedId]
public partial record SampleRecord;

[StronglyTypedId(StringTransformation.ToUpper)]
public partial record UpperRecord;

[StronglyTypedId(validator: typeof(ValidatedRecordValidator))]
public partial record ValidatedRecord;

internal class ValidatedRecordValidator : IValidator
{
    public bool Validate(string value)
    {
        return value.Length > 5;
    }
}