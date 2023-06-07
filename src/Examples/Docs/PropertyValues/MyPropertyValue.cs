﻿using Nikcio.UHeadless.Base.Properties.Commands;
using Nikcio.UHeadless.Base.Properties.Models;

namespace Examples.Docs.PropertyValues;

public class MyPropertyValue : PropertyValue
{
    public string? Name { get; set; }

    public MyPropertyValue(CreatePropertyValue createPropertyValue) : base(createPropertyValue)
    {
        var value = createPropertyValue.Property.GetValue(createPropertyValue.Culture);
        if (value == null)
        {
            return;
        }

        Name = (string) value;
    }
}
