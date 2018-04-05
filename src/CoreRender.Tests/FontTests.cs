using CoreRender.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CoreRender.Tests
{
    [TestClass]
    public class FontTests
    {
        [TestMethod]
        public void CreateFontTest()
        {
            Console.WriteLine(Font.CreateFont(800));
        }
    }
}
