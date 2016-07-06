﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NHS111.Domain.Glossary;
using NHS111.Domain.Glossary.Configuration;
using NUnit.Framework;
namespace NHS111.Domain.Glossary.Tests
{
    [TestFixture()]
    public class DefinitionRepositoryTests
    {

        private string testCsvContent = "GLOSSARYTERM,DESCRIPTION,SYNONYMS" + Environment.NewLine +
                                        "Abdomen,The abdomen relates to the part of the body that extends from underneath the breast area to just above the genitals.,\\r\\n" + Environment.NewLine +
                                        "Acne,\"A common skin condition which causes spots and pimples which occur mainly on the face, arms, chest and back. It is caused when the skin produces an oily substance known as sebum. Sebum can block the hair follicles and cause spots.\",\\r\\n" + Environment.NewLine +
                                        "Agitated,\"A mental state where you feel anxious, irritated and you are not calm.\",Agitation\\r\\n";

        Mock<IFileAdapter> _fileAdapterMock = new Mock<IFileAdapter>();
        [Test()]
        public void DefinitionRepository_List_Test()
        {
            _fileAdapterMock.Setup(f => f.OpenText()).Returns(GetCsVContentStream());
            var definitionRepository = new DefinitionRepository(new CsvRepostory<DefinitionsMap>(_fileAdapterMock.Object));

            var result = definitionRepository.List();

            Assert.IsNotNull(result);
            Assert.AreEqual(3,result.Count());

            Assert.AreEqual("Abdomen", result.First().Term);


        }

     

        private StreamReader GetCsVContentStream()
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(testCsvContent);
            MemoryStream stream = new MemoryStream(byteArray);
            return new StreamReader(stream);
        }
    }
}
