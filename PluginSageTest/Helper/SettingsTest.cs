using System;
using PluginSage.Helper;
using Xunit;

namespace PluginSageTest.Helper
{
    public class SettingsTest
    {
        [Fact]
        public void ValidateTest()
        {
            // setup
            var settings = new Settings
            {
                Username = "user",
                Password = "pass",
                CompanyCode = "TST",
                HomePath = "path",
                ModulesList = new []
                {
                    "one",
                    "two"
                }
            };
            
            // act
            settings.Validate();

            // assert
        }
        
        [Fact]
        public void ValidateNullUsernameTest()
        {
            // setup
            var settings = new Settings
            {
                Username = null,
                Password = "pass",
                CompanyCode = "TST",
                HomePath = "path",
                ModulesList = new []
                {
                    "one",
                    "two"
                }
            };
            
            // act
            Exception e  = Assert.Throws<Exception>(() => settings.Validate());

            // assert
            Assert.Contains("the Username property must be set", e.Message);
        }
        
        [Fact]
        public void ValidateNullPasswordTest()
        {
            // setup
            var settings = new Settings
            {
                Username = "user",
                Password = null,
                CompanyCode = "TST",
                HomePath = "path",
                ModulesList = new []
                {
                    "one",
                    "two"
                }
            };
            
            // act
            Exception e  = Assert.Throws<Exception>(() => settings.Validate());

            // assert
            Assert.Contains("the Password property must be set", e.Message);
        }
        
        [Fact]
        public void ValidateNullCompanyCodeTest()
        {
            // setup
            var settings = new Settings
            {
                Username = "user",
                Password = "pass",
                CompanyCode = null,
                HomePath = "path",
                ModulesList = new []
                {
                    "one",
                    "two"
                }
            };
            
            // act
            Exception e  = Assert.Throws<Exception>(() => settings.Validate());

            // assert
            Assert.Contains("the CompanyCode property must be set", e.Message);
        }
        
        [Fact]
        public void ValidateNullHomePathTest()
        {
            // setup
            var settings = new Settings
            {
                Username = "user",
                Password = "pass",
                CompanyCode = "TST",
                HomePath = null,
                ModulesList = new []
                {
                    "one",
                    "two"
                }
            };
            
            // act
            Exception e  = Assert.Throws<Exception>(() => settings.Validate());

            // assert
            Assert.Contains("the HomePath property must be set", e.Message);
        }
    }
}