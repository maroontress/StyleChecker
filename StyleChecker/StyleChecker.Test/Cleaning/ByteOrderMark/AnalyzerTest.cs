namespace StyleChecker.Test.Cleaning.ByteOrderMark
{
    using System.IO;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using StyleChecker.Cleaning.ByteOrderMark;
    using StyleChecker.Test.Framework;

    [TestClass]
    public sealed class AnalyzerTest : DiagnosticVerifier
    {
        private static readonly string BasePath
            = Path.Combine(Categories.Cleaning, Analyzer.DiagnosticId);

        public AnalyzerTest()
            : base(BasePath, new Analyzer())
        {
        }

        [TestMethod]
        public void NotFound()
        {
            var code = @"";
            var path = Path.Combine(BasePath, "Test0.cs");
            File.Delete(path);

            var atmosphere = Atmosphere.Default
                .WithBasePath(BasePath)
                .WithForceLocationValid(true);
            var result = NewErrorResult(
                NewLocations(1, 1),
                Analyzer.DiagnosticId,
                $"File not found: {path}",
                DiagnosticSeverity.Error);
            VerifyDiagnostic(code, atmosphere, result);
        }

        [TestMethod]
        public void Empty()
        {
            var code = @"";
            var path = Path.Combine(BasePath, "Test0.cs");
            File.WriteAllText(path, code, Encoding.ASCII);

            var atmosphere = Atmosphere.Default
                .WithBasePath(BasePath);
            VerifyDiagnostic(code, atmosphere);
        }

        [TestMethod]
        public void Okay()
        {
            var binPath = Path.Combine(BasePath, "Okay.bin");
            var code = File.ReadAllText(binPath);
            var bin = File.ReadAllBytes(binPath);
            var path = Path.Combine(BasePath, "Test0.cs");
            File.WriteAllBytes(path, bin);

            var atmosphere = Atmosphere.Default
                .WithBasePath(BasePath);
            VerifyDiagnostic(code, atmosphere);
        }

        [TestMethod]
        public void OkayCustomFile()
        {
            var binPath = Path.Combine(BasePath, "Okay.bin");
            var code = File.ReadAllText(binPath);
            var bin = File.ReadAllBytes(binPath);
            var path = Path.Combine(BasePath, "Test0.cs");
            File.WriteAllBytes(path, bin);

            var customPath = Path.Combine(BasePath, "CustomFile");
            File.WriteAllBytes(
                customPath,
                File.ReadAllBytes(Path.Combine(BasePath, "Okay.bin")));

            var configText = ReadText("SpecifyCustomFile", "xml");

            var atmosphere = Atmosphere.Default
                .WithConfigText(configText)
                .WithBasePath(BasePath);
            VerifyDiagnostic(code, atmosphere);
        }

        [TestMethod]
        public void FileStartsWithBom()
        {
            var binPath = Path.Combine(BasePath, "Code.bin");
            var code = File.ReadAllText(binPath);
            var bin = File.ReadAllBytes(binPath);
            var path = Path.Combine(BasePath, "Test0.cs");
            File.WriteAllBytes(path, bin);

            var atmosphere = Atmosphere.Default
                .WithBasePath(BasePath)
                .WithForceLocationValid(true);
            var result = NewErrorResult(
                NewLocations(1, 1),
                Analyzer.DiagnosticId,
                $"The BOM in UTF-8 encoding must be removed: {path}");
            VerifyDiagnostic(code, atmosphere, result);
        }

        [TestMethod]
        public void CustomFileStartsWithBom()
        {
            var binPath = Path.Combine(BasePath, "Okay.bin");
            var code = File.ReadAllText(binPath);
            var bin = File.ReadAllBytes(binPath);
            var path = Path.Combine(BasePath, "Test0.cs");
            File.WriteAllBytes(path, bin);

            var customPath = Path.Combine(BasePath, "CustomFile");
            File.WriteAllBytes(
                customPath,
                File.ReadAllBytes(Path.Combine(BasePath, "Code.bin")));

            var configText = ReadText("SpecifyCustomFile", "xml");

            var atmosphere = Atmosphere.Default
                .WithConfigText(configText)
                .WithBasePath(BasePath);

            var result = NewErrorResult(
                NewNoLocations(),
                Analyzer.DiagnosticId,
                $"The BOM in UTF-8 encoding must be removed: {customPath}");
            VerifyDiagnostic(code, atmosphere, result);
        }

        private static ResultLocation[] NewNoLocations()
            => Arrays.Create(new ResultLocation(null, -1, -1));

        private static ResultLocation[] NewLocations(int row, int col)
        {
            var path = Path.Combine(BasePath, "Test0.cs");
            return Arrays.Create(new ResultLocation(path, row, col));
        }

        private static Result NewErrorResult(
            ResultLocation[] locations,
            string id,
            string message,
            DiagnosticSeverity severity = DiagnosticSeverity.Warning)
            => new Result(locations, id, message, severity);
    }
}
