using System;
using System.IO;
using System.Linq;

namespace FI.ControlM
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string ArquivoXml = string.Empty;
            string DirSaida = string.Empty;
            string TipoGeracao = string.Empty;
            string valorParam;

            if ((args.Count() == 0) || ((args.Count() == 1) && (args[0].Contains("?") || args[0].ToLower().Contains("help"))))
            {
                Console.WriteLine("");
                Console.WriteLine("**************************************************************");
                Console.WriteLine("");
                Console.WriteLine("Ajuda execução, parametros permitidos: ");
                Console.WriteLine("");
                Console.WriteLine("    [XMLFILE]         - OBRIGATÓRIO - Diretório e nome do arquivo Xml exportado pelo Control-M (Panning -> Workspace -> Export)");
                Console.WriteLine("    [FOLDER_NAME]     - OBRIGATÓRIO FILTRO - Nome Pasta no ControlM - Tag <DEFTABLE><FOLDER FOLDERNAME='????'>... do Xml");
                Console.WriteLine("    [TPGERACAO]       - Tipo de geração (B - Arquivos .Bat ou F - Fluxograma ou não informar para gerar ambos)");
                Console.WriteLine("    [DIR_SAIDA]       - Diretório onde serão gerados os arquivos processados");
                Console.WriteLine("    [APPLICATION]     - FILTRO - Nome Aplicação no ControlM - Tag <DEFTABLE><FOLDER><JOB APPLICATION='???'>... do Xml");
                Console.WriteLine("    [SUB_APPLICATION] - FILTRO - Nome Sub-Aplicação no ControlM - Tag <DEFTABLE><FOLDER><JOB SUBAPPLICATION='???'>... do Xml");
                Console.WriteLine("    [MEMNAME]         - FILTRO - Nome do Exe no ControlM - Tag <DEFTABLE><FOLDER><JOB MEMNAME='???'>... do Xml");
                Console.WriteLine("    [JOBNAME]         - FILTRO - Nome técnico do JOB Tag <DEFTABLE><FOLDER><JOB JOBNAME='???'>... do Xml");
                Console.WriteLine("    [ROTINA]          - FILTRO - Nome técnico da rotina Tag <DEFTABLE><FOLDER><JOB><VARIABLE NAME=\"%%PARM2\" VALUE=\"???\"/>... do Xml");
                Console.WriteLine("    [DEPENDENCIAS]    - FILTRO - Retornar dependencias da rotina Sim|Nao");
                Console.WriteLine("");
                Console.WriteLine("Ex.: FI.ControlM.exe XMLFILE=\"C:\\temp\\WorkspaceExported.Xml\" DIR_SAIDA=\"C:\\ArquivosGerados\\\" SUBAPPLICATION=\"CSG_TOMBAMENTO\" ...");
                Console.WriteLine("");
                Console.WriteLine("**************************************************************");
                Console.WriteLine("");
                return;
            }

            FiltrosControlM filtrosControlM = new FiltrosControlM();
            Console.WriteLine("Parametros informados: ");
            foreach (string arg in args)
            {
                Console.WriteLine(arg);
                valorParam = string.Empty;
                if (arg.Contains("="))
                    valorParam = arg.Split('=')[1].Trim();
                if (!string.IsNullOrWhiteSpace(valorParam))
                {
                    if (arg.Contains("XMLFILE"))
                        ArquivoXml = valorParam;
                    else if (arg.Contains("SUB_APPLICATION"))
                        filtrosControlM.SUBAPPLICATION = valorParam;
                    else if (arg.Contains("APPLICATION"))
                        filtrosControlM.APPLICATION = valorParam;
                    else if (arg.Contains("FOLDER_NAME"))
                        filtrosControlM.FOLDERNAME = valorParam;
                    else if (arg.Contains("JOBNAME"))
                        filtrosControlM.JOBNAME = valorParam;
                    else if (arg.Contains("MEMNAME"))
                        filtrosControlM.MEMNAME = valorParam;
                    else if (arg.Contains("ROTINA"))
                        filtrosControlM.ROTINA = valorParam;
                    else if (arg.Contains("DIR_SAIDA"))
                        DirSaida = valorParam;
                    else if (arg.Contains("TPGERACAO"))
                        TipoGeracao = valorParam;
                    else if (arg.Contains("DEPENDENCIAS"))
                        filtrosControlM.DEPENDENCIAS = valorParam.ToLower() == "sim";
                }
            }
            if (string.IsNullOrWhiteSpace(ArquivoXml))
                Console.WriteLine("Nome do arquivo Xml obrigatório!");
            else if (!File.Exists(ArquivoXml))
                Console.WriteLine("Arquivo Xml não existe!");
            else
            {
                if (TipoGeracao == "F" || string.IsNullOrWhiteSpace(TipoGeracao))
                {
                    Console.WriteLine("Gerar Fluxograma");
                    new BoFIControlM(ArquivoXml, filtrosControlM, DirSaida).GerarFluxograma();
                }
                if (TipoGeracao == "B" || string.IsNullOrWhiteSpace(TipoGeracao))
                {
                    Console.WriteLine("Gerar Arquivos Bat");
                    new BoFIControlM(ArquivoXml, filtrosControlM, DirSaida, DateTime.Now).GerarArquivosBat();
                }
                Console.WriteLine("Processo finalizado!");
            }
        }
    }
}