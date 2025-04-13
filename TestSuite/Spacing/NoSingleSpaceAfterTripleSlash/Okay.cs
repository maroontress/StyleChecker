namespace StyleChecker.Test.Spacing.NoSingleSpaceAfterTripleSlash
{
    public sealed class Okay
    {
        ///
        private void Empty()
        {
        }

        /// <summary>
        /// summary.
        /// </summary>
        /// <param name="x">
        /// parameter.
        /// </param>
        private void NoProblem(int x)
        {
        }

        /// <summary>
        /// </summary>
        ///
        private void LastEmpty()
        {
        }

        ///
        /// <summary>
        /// </summary>
        private void FirstEmpty()
        {
        }

        /// <summary>
        ///
        /// </summary>
        private void IncludeEmpty()
        {
        }

        /// Hello
        private void NoXml()
        {
        }

        ///  Hello
        private void NoXmlExtraIndent()
        {
        }

        /// header
        /// <summary>Hello, world</summary>
        /// footer
        private void OutSideXml()
        {
        }

        ///  header
        /// <summary>Hello, world</summary>
        ///  footer
        private void OutSideXmlExtraIndent()
        {
        }

        /// <summary>
        ///	header
        /// </summary>
        private void Tab()
        {
        }

        ///	header
        private void NoXmlTab()
        {
        }

        private void NoDocumentationComment()
        {
            ///  

            /// hello

            ///  hello

            /// hello <see cref="world"/>

            ///  hello <see cref="world"/>

            ///	 

            ///	hello

            ///	 hello

            ///	hello <see cref="world"/>

            ///	 hello <see cref="world"/>
        }

        /// <example>
        /// <code><![CDATA[
        /// Hello world
        /// ]]></code>
        /// </example>
        private void CDataSection()
        {
        }

        /// <example>
        /// <code><![CDATA[
        ///   <PropertyGroup>
        ///     <TargetFramework>net8</TargetFramework>
        ///     <LangVersion>12.0</LangVersion>
        ///     <Nullable>enable</Nullable>
        ///   </PropertyGroup>
        /// ]]></code>
        /// </example>
        private void XmlInCDataSection()
        {
        }

        /// <![CDATA[
        /// Ignored
        /// ]]>
        private void CDataSectionOutsideXml()
        {
            /// <![CDATA[
            /// Ignored
            /// ]]>
        }

        /// <summary>Print "Hello world!"</summary>
        /// <example>
        /// <![CDATA[
        /// #include <iostream>
        ///
        /// int main() {
        ///     std::cout << "Hello world!" << std::endl;
        ///
        ///     return 0;
        /// }
        /// ]]>
        /// </example>
        private void CDataSectionContainingEmptyLineWithoutTrailingSpace()
        {
        }
    }
}
