using System;
using System.ComponentModel;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

namespace IMagic.Core.Tests.Enum;



public enum ExternalResourceFileConstants
{
    [Description("det_500m.onnx")]
    DetectionModel,

    [Description("w600k_r50.onnx")]
    RecognitionModel,

    [Description("cities1000.txt")]
    GeoNamesData,

    // If MissingDescription is a real resource, add a Description; if not, remove it.
    // For now, we'll keep it to test fallback, but the enforcement test will skip it.
    MissingDescription
}

// Separate test enum for edge cases (zero value, no description, etc.)
public enum TestEnumWithZeroValue
{
    [Description("zero_file.onnx")]
    ZeroWithDescription = 0
}

public enum TestEnumWithoutDescription
{
    ZeroValue = 0
}

[TestClass]
public class EnumDescriptionTests
{

    [TestMethod]
    public void WhenEnumHasDescription_ReturnsDescription()
    {
        var resource = ExternalResourceFileConstants.DetectionModel;
        Assert.AreEqual("det_500m.onnx", resource.EnumDescription());
    }

    [TestMethod]
    public void WhenEnumHasNoDescriptionAndNoFallback_Throws()
    {
        var resource = ExternalResourceFileConstants.MissingDescription;
        Assert.ThrowsExactly<InvalidOperationException>(() => resource.EnumDescription());
    }

    [TestMethod]
    public void WhenEnumHasNoDescriptionAndFallbackEnabled_ReturnsEnumName()
    {
        var resource = ExternalResourceFileConstants.MissingDescription;
        var result = resource.EnumDescription(fallbackToEnumName: true);
        Assert.AreEqual("MissingDescription", result);
    }

    [TestMethod]
    public void WhenEnumValueIsZeroAndHasDescription_ReturnsDescription()
    {
        var resource = TestEnumWithZeroValue.ZeroWithDescription;
        var result = resource.EnumDescription();
        Assert.AreEqual("zero_file.onnx", result);
    }

    [TestMethod]
    public void WhenEnumValueIsZeroAndNoDescriptionAndNoFallback_Throws()
    {
        var resource = TestEnumWithoutDescription.ZeroValue;
        Assert.ThrowsExactly<InvalidOperationException>(() => resource.EnumDescription());
    }

    [TestMethod]
    public void WhenEnumValueIsZeroAndNoDescriptionAndFallbackEnabled_ReturnsEnumName()
    {
        var resource = TestEnumWithoutDescription.ZeroValue;
        var result = resource.EnumDescription(fallbackToEnumName: true);
        Assert.AreEqual("ZeroValue", result);
    }

    [TestMethod]
    public void WhenEnumIsNull_ThrowsArgumentNull()
    {
        ExternalResourceFileConstants? nullEnum = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => nullEnum.EnumDescription());
    }

    [TestMethod]
    public void EnforcesDescriptionAttributeOnAllProductionResourceEnums()
    {
        // Only enforce for enum members that are expected to have descriptions.
        // We skip MissingDescription for this test because it's intentionally missing.
        var membersToCheck = new[]
        {
                ExternalResourceFileConstants.DetectionModel,
                ExternalResourceFileConstants.RecognitionModel,
                ExternalResourceFileConstants.GeoNamesData
            };
        foreach (var value in membersToCheck)
        {
            var result = value.EnumDescription(); // should not throw
            Assert.IsNotNull(result);
            Assert.AreNotEqual(value.ToString(), result);
        }
    }
}