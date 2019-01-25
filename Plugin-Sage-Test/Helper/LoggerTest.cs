using System.IO;
using Plugin_Sage.Helper;
using Xunit;

namespace Plugin_Sage_Test.Helper
{
    public class LoggerTest
    {
        [Fact]
        public void VerboseTest()
        {
            // setup
            File.Delete(@"plugin.txt");
            Logger.SetLogLevel(Logger.LogLevel.Verbose);
            
            // act
            Logger.Verbose("verbose");
            Logger.Debug("debug");
            Logger.Info("info");
            Logger.Error("error");

            // assert
            string[] lines = File.ReadAllLines(@"plugin.txt");

            Assert.Equal(4, lines.Length);
            
            // cleanup
            File.Delete(@"plugin.txt");
        }
        
        [Fact]
        public void DebugTest()
        {
            // setup
            File.Delete(@"plugin.txt");
            Logger.SetLogLevel(Logger.LogLevel.Debug);
            
            // act
            Logger.Verbose("verbose");
            Logger.Debug("debug");
            Logger.Info("info");
            Logger.Error("error");

            // assert
            string[] lines = File.ReadAllLines(@"plugin.txt");

            Assert.Equal(3, lines.Length);
            
            // cleanup
            File.Delete(@"plugin.txt");
        }
        
        [Fact]
        public void InfoTest()
        {
            // setup
            File.Delete(@"plugin.txt");
            Logger.SetLogLevel(Logger.LogLevel.Info);
            
            // act
            Logger.Verbose("verbose");
            Logger.Debug("debug");
            Logger.Info("info");
            Logger.Error("error");

            // assert
            string[] lines = File.ReadAllLines(@"plugin.txt");

            Assert.Equal(2, lines.Length);
            
            // cleanup
            File.Delete(@"plugin.txt");
        }
        
        [Fact]
        public void ErrorTest()
        {
            // setup
            File.Delete(@"plugin.txt");
            Logger.SetLogLevel(Logger.LogLevel.Error);
            
            // act
            Logger.Verbose("verbose");
            Logger.Debug("debug");
            Logger.Info("info");
            Logger.Error("error");

            // assert
            string[] lines = File.ReadAllLines(@"plugin.txt");

            Assert.Single(lines);
            
            // cleanup
            File.Delete(@"plugin.txt");
        }
        
        [Fact]
        public void OffTest()
        {
            // setup
            File.Delete(@"plugin.txt");
            Logger.SetLogLevel(Logger.LogLevel.Off);
            
            // act
            Logger.Verbose("verbose");
            Logger.Debug("debug");
            Logger.Info("info");
            Logger.Error("error");

            // assert
            string[] lines = File.Exists(@"plugin.txt") ? File.ReadAllLines(@"plugin.txt") : new string[0];

            Assert.Empty(lines);
            
            // cleanup
            File.Delete(@"plugin.txt");
        }
    }
}