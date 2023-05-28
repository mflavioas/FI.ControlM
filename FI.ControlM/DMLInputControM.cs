using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace FI.ControlM
{
    [XmlRoot(ElementName = "VARIABLE")]
    public class VARIABLE
    {

        [XmlAttribute(AttributeName = "NAME")]
        public string NAME;

        [XmlAttribute(AttributeName = "VALUE")]
        public string VALUE;
    }

    [XmlRoot(ElementName = "OUTCOND")]
    public class OUTCOND
    {
        private string _NAME;
        public string NameDe { get; set; }
        public string NamePara { get; set; }

        [XmlAttribute(AttributeName = "NAME")]
        public string NAME
        {
            get
            {
                return _NAME;
            }
            set
            {
                _NAME = value.Replace("-TO-", "-").Replace("--", "-");
                string[] NameDePara = _NAME.Split('-');
                NameDe = NameDePara[0];
                NamePara = NameDePara[1];
            }
        }

        [XmlAttribute(AttributeName = "ODATE")]
        public string ODATE;

        [XmlAttribute(AttributeName = "SIGN")]
        public string SIGN;
    }

    [XmlRoot(ElementName = "INCOND")]
    public class INCOND
    {
        private string _NAME;
        public string NameDe { get; set; }
        public string NamePara { get; set; }

        [XmlAttribute(AttributeName = "NAME")]
        public string NAME
        {
            get
            {
                return _NAME;
            }
            set
            {
                _NAME = value.Replace("-TO-", "-").Replace("--", "-");
                string[] NameDePara = _NAME.Split('-');
                NameDe = NameDePara[0];
                NamePara = NameDePara[1];
            }
        }

        [XmlAttribute(AttributeName = "ODATE")]
        public string ODATE;

        [XmlAttribute(AttributeName = "AND_OR")]
        public string AND_OR;
    }

    [XmlRoot(ElementName = "JOB")]
    public class JOB
    {
        public string Rotina
        {
            get
            {
                string retorno = string.Empty;
                if (VARIABLE.Count > 1)
                    retorno = VARIABLE[1].VALUE;
                return retorno;
            }
        }

        [XmlElement(ElementName = "VARIABLE")]
        public List<VARIABLE> VARIABLE;

        [XmlElement(ElementName = "SHOUT")]
        public List<object> SHOUT;

        [XmlElement(ElementName = "QUANTITATIVE")]
        public List<object> QUANTITATIVE;

        [XmlElement(ElementName = "OUTCOND")]
        public List<OUTCOND> OUTCOND;

        [XmlElement(ElementName = "INCOND")]
        public List<INCOND> INCOND;

        [XmlAttribute(AttributeName = "JOBISN")]
        public int JOBISN;

        [XmlAttribute(AttributeName = "APPLICATION")]
        public string APPLICATION;

        [XmlAttribute(AttributeName = "SUB_APPLICATION")]
        public string SUBAPPLICATION;

        [XmlAttribute(AttributeName = "MEMNAME")]
        public string MEMNAME;

        [XmlAttribute(AttributeName = "JOBNAME")]
        public string JOBNAME;

        private string _DESCRIPTION;
        [XmlAttribute(AttributeName = "DESCRIPTION")]
        public string DESCRIPTION
        {
            get { return _DESCRIPTION; }
            set { _DESCRIPTION = Regex.Replace(value, @"\t|\n|\r", " ").Replace("\"", "").Trim(); }
        }

        [XmlAttribute(AttributeName = "MEMLIB")]
        public string MEMLIB;

        [XmlAttribute(AttributeName = "APPL_TYPE")]
        public string APPL_TYPE;

        [XmlAttribute(AttributeName = "PARENT_FOLDER")]
        public string PARENT_FOLDER;        
    }

    [XmlRoot(ElementName = "FOLDER")]
    public class FOLDER
    {

        [XmlElement(ElementName = "JOB")]
        public List<JOB> JOB;

        [XmlAttribute(AttributeName = "FOLDER_NAME")]
        public string FOLDERNAME;
    }

    [XmlRoot(ElementName = "DEFTABLE")]
    public class ControlM
    {
        [XmlElement(ElementName = "WORKSPACE")]
        public List<object> WORKSPACE;

        [XmlElement(ElementName = "SMART_FOLDER")]
        public List<object> SMART_FOLDER;

        [XmlElement(ElementName = "FOLDER")]
        public List<FOLDER> FOLDER;
    }

    public class FiltrosControlM
    {
        public string APPL_TYPE { get { return "OS"; } }
        public string APPLICATION { get; set; }
        public string FOLDERNAME { get; set; }
        public string SUBAPPLICATION { get; set; }
        public string JOBNAME { get; set; }
        public string MEMNAME { get; set; }
        public string ROTINA { get; set; }
        public bool DEPENDENCIAS { get; set; }
    }

    public class RetornoBat
    {
        public string SubDiretorio { get; set; }
        public string NomeArquivo { get; set; }
        public List<string> LinhaComando = new List<string>();
    }
}
