using PlantUml.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Configuration;

namespace FI.ControlM
{
    public class BoFIControlM
    {
        const string BaseArq = "\\CSG\\";
        const string NomeExe_NET = "FI.CSG.LinhaDeComando";

        const string VAR_ODATE = "%%$ODATE.";
        const string VAR_TIME = "%%TIME.";
        const string VAR_OYEAR = "%%$OYEAR.";
        const string VAR_OMONTH = "%%OMONTH.";
        const string VAR_ODAY = "%%ODAY.";

        private List<RetornoBat> retornoBats = new List<RetornoBat>();
        private StringBuilder ModeloFluxograma = new StringBuilder();
        private OutputFormat outputFormat = OutputFormat.Svg;
        private ControlM controlM;
        private string ArquivoXmlExportadoControlM { get; set; }
        private FiltrosControlM filtrosControlM { get; set; }
        private string NomeArquivoFluxograma
        {
            get
            {
                string extencao;
                if (outputFormat != OutputFormat.Svg && outputFormat != OutputFormat.Png)
                    outputFormat = OutputFormat.Svg;
                switch (outputFormat)
                {
                    case OutputFormat.Png:
                        extencao = ".png";
                        break;
                    case OutputFormat.Svg:
                        extencao = ".svg";
                        break;
                    default:
                        extencao = ".svg";
                        break;
                }
                string ret = "Fluxo";
                if (!string.IsNullOrWhiteSpace(filtrosControlM.FOLDERNAME))
                    ret += $"_{filtrosControlM.FOLDERNAME}";
                if (!string.IsNullOrWhiteSpace(filtrosControlM.APPLICATION))
                    ret += $"_{filtrosControlM.APPLICATION}";
                if (!string.IsNullOrWhiteSpace(filtrosControlM.SUBAPPLICATION))
                    ret += $"_{filtrosControlM.SUBAPPLICATION}";
                if (!string.IsNullOrWhiteSpace(filtrosControlM.JOBNAME))
                    ret += $"_{filtrosControlM.JOBNAME}";
                if (!string.IsNullOrWhiteSpace(filtrosControlM.ROTINA))
                    ret += $"_{filtrosControlM.ROTINA}";
                return ret += extencao;
            }
        }
        private string DiretorioDestino { get; set; }
        private DateTime ODate { get; set; }
        public string ConfigDiarArquivos { get { return ConfigurationManager.AppSettings["DirArquivosLocal"]; } }
        public string ConfigDirExeLC_NET { get { return ConfigurationManager.AppSettings["DirExeLC_NET"]; } }
        public string ConfigDirExeLC_Delphi { get { return ConfigurationManager.AppSettings["DirExeLC_Delphi"]; } }

        public BoFIControlM(string pArquivoXmlExportadoControlM, FiltrosControlM pFiltrosControlM, string pDiretorioDestino)
        {
            ArquivoXmlExportadoControlM = pArquivoXmlExportadoControlM;
            filtrosControlM = pFiltrosControlM;
            DiretorioDestino = pDiretorioDestino;
            ODate = DateTime.Now;
        }
        public BoFIControlM(string pArquivoXmlExportadoControlM, FiltrosControlM pFiltrosControlM, string pDiretorioDestino, DateTime pODate)
        {
            ArquivoXmlExportadoControlM = pArquivoXmlExportadoControlM;
            filtrosControlM = pFiltrosControlM;
            DiretorioDestino = pDiretorioDestino;
            ODate = pODate;
        }
        private ControlM CarregaXml()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ControlM));
            var reader = XmlReader.Create(ArquivoXmlExportadoControlM);
            return (ControlM)serializer.Deserialize(reader);
        }
        private bool isValidJOB(JOB job)
        {
            return (
                 (string.IsNullOrWhiteSpace(filtrosControlM.APPL_TYPE) || job.APPL_TYPE == filtrosControlM.APPL_TYPE)
                && (job.MEMNAME.ToUpper().Contains(".EXE"))
                );
        }
        private string TratarVariavel(string variavel)
        {
            if (variavel.ToUpper().Contains(BaseArq))
            {
                foreach (string Config_DIRSERVER in ConfigurationManager.AppSettings)
                {
                    if (Config_DIRSERVER.Contains("DIRSERVER"))
                    {
                        variavel = variavel.ToUpper().Replace(ConfigurationManager.AppSettings[Config_DIRSERVER], ConfigDiarArquivos);
                    }
                }
            }
            return variavel.Replace(VAR_ODATE, ODate.ToString("yyyyMMdd"))
                           .Replace(VAR_TIME, DateTime.Now.ToString("HHmmss"))
                           .Replace(VAR_OYEAR, ODate.ToString("yyyy"))
                           .Replace(VAR_OMONTH, ODate.ToString("MM"))
                           .Replace(VAR_ODAY, ODate.ToString("dd"));
        }
        private List<JOB> RetornaListaJobsDePara(IEnumerable<JOB> jobsFULL, IEnumerable<JOB> jobs, IEnumerable<JOB> retorno)
        {
            List<JOB> dependentes = new List<JOB>();
            foreach (JOB job in jobs)
            {
                foreach (INCOND LigacaoJob in job.INCOND)
                {
                    dependentes.AddRange(jobsFULL.Where(d => d.JOBNAME == LigacaoJob.NameDe && !retorno.Contains(d)));
                }
            }
            return dependentes;
        }
        private IEnumerable<JOB> RetornaListaJobs(IEnumerable<JOB> jobs)
        {
            if (!string.IsNullOrWhiteSpace(filtrosControlM.JOBNAME) || !string.IsNullOrWhiteSpace(filtrosControlM.ROTINA)
                 || !string.IsNullOrWhiteSpace(filtrosControlM.MEMNAME))
            {
                List<JOB> retorno = new List<JOB>();
                if (!string.IsNullOrWhiteSpace(filtrosControlM.JOBNAME))
                    retorno.AddRange(jobs.Where(j => j.JOBNAME == filtrosControlM.JOBNAME));
                if (!string.IsNullOrWhiteSpace(filtrosControlM.ROTINA))
                    retorno.AddRange(jobs.Where(j => j.Rotina == filtrosControlM.ROTINA && !retorno.Contains(j)));
                if (!string.IsNullOrWhiteSpace(filtrosControlM.MEMNAME))
                    retorno.AddRange(jobs.Where(j => j.MEMNAME == filtrosControlM.MEMNAME && !retorno.Contains(j)));
                if (filtrosControlM.DEPENDENCIAS)
                {
                    List<JOB> recursivo = RetornaListaJobsDePara(jobs, retorno, retorno);
                    while (recursivo.Count > 0)
                    {
                        retorno.AddRange(recursivo);
                        recursivo = RetornaListaJobsDePara(jobs, recursivo, retorno);
                    }
                }
                return retorno;
            }
            else
                return jobs;
        }
        private void RetornaLinhaCmd(IEnumerable<JOB> ListaJob, JOB job)
        {
            RetornoBat BatLC = new RetornoBat();
            JOB subDirJob = CarregaMinIDJob(ListaJob, job.JOBNAME);
            BatLC.SubDiretorio = $"{subDirJob.JOBISN}_{subDirJob.SUBAPPLICATION}";
            BatLC.NomeArquivo = $"{job.JOBISN.ToString("000")}_{job.APPLICATION}_{job.JOBNAME}_{job.Rotina}.bat".ToUpper();
            BatLC.LinhaComando.Add($"REM {job.JOBNAME} : {job.JOBISN} - {job.DESCRIPTION}");
            string ExeName = string.Concat(ConfigDirExeLC_Delphi, job.MEMNAME);
            if (job.MEMNAME.Contains(NomeExe_NET))
                ExeName = string.Concat(ConfigDirExeLC_NET, job.MEMNAME);
            string linhaCmd = string.Concat(ExeName);
            foreach (VARIABLE variavel in job.VARIABLE)
            {
                linhaCmd += string.Concat(" ", TratarVariavel(variavel.VALUE));
            }
            BatLC.LinhaComando.Add($"{linhaCmd}");
            BatLC.LinhaComando.Add("echo %errorlevel%");
            BatLC.LinhaComando.Add("pause");
            retornoBats.Add(BatLC);
        }
        private List<string> CarregaJobSubAplicacao(IEnumerable<JOB> ListaJob)
        {
            List<string> sbJOB = new List<string>();

            var grpSubAplicacao = from subaplicacao in ListaJob
                                  where (subaplicacao.SUBAPPLICATION == filtrosControlM.SUBAPPLICATION || string.IsNullOrWhiteSpace(filtrosControlM.SUBAPPLICATION)) && isValidJOB(subaplicacao)
                                  group subaplicacao by subaplicacao.SUBAPPLICATION;

            foreach (var itemgrpSubAplicacao in grpSubAplicacao)
            {
                sbJOB.Add(string.Concat("package \"",
                    itemgrpSubAplicacao.First(x => 1 == 1).PARENT_FOLDER, "_",
                    itemgrpSubAplicacao.First(x => 1 == 1).APPLICATION, "_",
                    itemgrpSubAplicacao.Key, "\" {"));
                foreach (var itemSubAplicacao in itemgrpSubAplicacao)
                {
                    sbJOB.Add($"{itemSubAplicacao.JOBNAME} : {itemSubAplicacao.JOBISN} - {itemSubAplicacao.MEMNAME.ToUpper().Replace(".EXE", "")}.{itemSubAplicacao.Rotina} - {itemSubAplicacao.DESCRIPTION}");
                    RetornaLinhaCmd(ListaJob, itemSubAplicacao);
                    if (!(!filtrosControlM.DEPENDENCIAS && (!string.IsNullOrWhiteSpace(filtrosControlM.JOBNAME) || !string.IsNullOrWhiteSpace(filtrosControlM.ROTINA))))
                    {
                        foreach (INCOND dependencias in itemSubAplicacao.INCOND)
                        {
                            {
                                sbJOB.Add(string.Concat(dependencias.NameDe, "-->", dependencias.NamePara));
                            }
                        }
                    }
                }
                sbJOB.Add("}");
            }
            if (sbJOB.Count < 3)
                sbJOB.Clear();
            return sbJOB;
        }
        private JOB CarregaMinIDJob(IEnumerable<JOB> ListaJob, string Nomejob)
        {
            string NomeJobAntecedente = Nomejob;
            JOB job = ListaJob.Where(w => w.JOBNAME == Nomejob).FirstOrDefault();

            while (job.INCOND.Count > 0)
            {
                if (ListaJob.Where(w => w.JOBNAME == job.INCOND.FirstOrDefault().NameDe).FirstOrDefault() == null)
                    break;
                job = ListaJob.Where(w => w.JOBNAME == job.INCOND.FirstOrDefault().NameDe).FirstOrDefault();
            }
            return job;
        }
        private List<string> CarregaJobAplicacao(IEnumerable<JOB> ListaJob)
        {
            List<string> sbJOB = new List<string>();

            var grpAplicacao = from aplicacao in ListaJob
                               where (aplicacao.APPLICATION == filtrosControlM.APPLICATION || string.IsNullOrWhiteSpace(filtrosControlM.APPLICATION)) && isValidJOB(aplicacao)
                               group aplicacao by aplicacao.APPLICATION;

            foreach (var itemgrpAplicacao in grpAplicacao)
            {
                sbJOB.Add(string.Concat("package \"", itemgrpAplicacao.First(x => 1 == 1).PARENT_FOLDER, "_", itemgrpAplicacao.Key, "\" {"));
                sbJOB.AddRange(CarregaJobSubAplicacao(ListaJob.Where(w => w.APPLICATION == itemgrpAplicacao.Key && isValidJOB(w)).OrderBy(o => o.SUBAPPLICATION)));
                sbJOB.Add("}");
            }
            if (sbJOB.Count < 3)
                sbJOB.Clear();
            return sbJOB;
        }
        private List<string> CarregaFolder(FOLDER folder)
        {
            List<string> sbFolder = new List<string>
            {
                string.Concat("package \"", folder.FOLDERNAME, "\" {")
            };
            sbFolder.AddRange(CarregaJobAplicacao(RetornaListaJobs(folder.JOB.OrderBy(o => o.APPLICATION))));
            sbFolder.Add("}");
            if (sbFolder.Count < 3)
                sbFolder.Clear();
            return sbFolder;
        }
        private void CarregaModelo()
        {
            controlM = CarregaXml();
            List<string> sbControlM = new List<string>
            {
                "@startuml Tissu",
                $"title {Path.GetFileName(ArquivoXmlExportadoControlM)}"
            };
            foreach (FOLDER folder in controlM.FOLDER.Where(f => string.IsNullOrWhiteSpace(filtrosControlM.FOLDERNAME) || f.FOLDERNAME == filtrosControlM.FOLDERNAME))
            {
                sbControlM.AddRange(CarregaFolder(folder));
            }
            sbControlM.Add("@enduml");
            foreach (string item in sbControlM)
            {
                ModeloFluxograma.AppendLine(item);
            }
        }
        public void GerarArquivosBat()
        {
            CarregaModelo();
            string Diretorio;
            foreach (RetornoBat retornoBat in retornoBats)
            {
                Diretorio = DiretorioDestino;
                if (!string.IsNullOrWhiteSpace(retornoBat.SubDiretorio))
                {
                    Diretorio = Path.Combine(DiretorioDestino, retornoBat.SubDiretorio);
                    if (!Directory.Exists(Path.Combine(DiretorioDestino, retornoBat.SubDiretorio)))
                        Directory.CreateDirectory(Diretorio);
                }
                if (File.Exists(Path.Combine(Diretorio, retornoBat.NomeArquivo)))
                    File.Delete(Path.Combine(Diretorio, retornoBat.NomeArquivo));
                File.WriteAllLines(Path.Combine(Diretorio, retornoBat.NomeArquivo), retornoBat.LinhaComando);
            }
        }
        public void GerarFluxograma()
        {
            var factory = new RendererFactory();
            var renderer = factory.CreateRenderer(new PlantUmlSettings());
            CarregaModelo();
            try
            {
                if (File.Exists(Path.Combine(DiretorioDestino, NomeArquivoFluxograma)))
                    File.Delete(Path.Combine(DiretorioDestino, NomeArquivoFluxograma));
                byte[] bytes = renderer.Render(ModeloFluxograma.ToString(), outputFormat);
                File.WriteAllBytes(Path.Combine(DiretorioDestino, NomeArquivoFluxograma), bytes);
            }
            catch
            {
                Console.WriteLine($"Problemas ao gerar fluxograma, tente usar um filtro para sintetizar o retorno.");
            }
        }
    }
}
