// <copyright file="EmailWorkTest.cs">© 2014 Zachary Karpinski. All rights reserved.</copyright>

using System;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TDriver;

namespace TDriver
{
    [TestClass]
    [PexClass(typeof(EmailWork))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    public partial class EmailWorkTest
    {
    }
}
