using StronglyTypedIds;

namespace Sample;

[StronglyTypedId]
public partial record struct SampleStructRecord;

[StronglyTypedId(StringTransformation.ToUpper)]
public partial record struct UpperStructRecord;

[StronglyTypedId(validator: typeof(ValidatedStructRecordValidator))]
public partial record struct ValidatedStructRecord;

internal class ValidatedStructRecordValidator : IValidator
{
    public bool Validate(string value)
    {
        return value.Length > 5;
    }
}