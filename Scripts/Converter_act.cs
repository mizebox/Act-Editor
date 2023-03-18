using System;
using System.IO;
using System.Windows;
using ActEditor.Utils;
using ActEditor.Utils.Effects;
using GRF.Threading;
using GRF.Graphics;
using GRF;

public class Script
{
    public void Run()
    {
        // Seleciona a pasta de entrada de onde os arquivos .act serão lidos
        string inputFolderPath = OpenDialogs.GetFolderPath("Selecione a pasta de entrada");
        if (inputFolderPath == null) return;

        // Seleciona a pasta de saída para onde as imagens serão salvas
        string outputFolderPath = OpenDialogs.GetFolderPath("Selecione a pasta de saída");
        if (outputFolderPath == null) return;

        // Define o formato de saída como PNG
        ImageFileFormat outputFormat = ImageFileFormat.Png;

        // Percorre todos os arquivos .act na pasta de entrada
        string[] inputFiles = Directory.GetFiles(inputFolderPath, "*.act");
        foreach (string inputFile in inputFiles)
        {
            // Lê o arquivo .act
            Palette pal = new Palette(inputFile);

            // Cria uma imagem em branco do mesmo tamanho que o arquivo .act
            Image<BGRA32> img = new Image<BGRA32>(pal.Colors.Length, 1);

            // Preenche a imagem com as cores do arquivo .act
            for (int i = 0; i < pal.Colors.Length; i++)
            {
                img[i, 0] = new BGRA32(pal.Colors[i].R, pal.Colors[i].G, pal.Colors[i].B, 255);
            }

            // Salva a imagem em PNG na pasta de saída
            string outputFilePath = Path.Combine(outputFolderPath, Path.GetFileNameWithoutExtension(inputFile) + "." + outputFormat.ToString().ToLower());
            img.Save(outputFilePath, outputFormat);
        }

        // Exibe uma mensagem informando que a conversão foi concluída
        MessageBox.Show("Conversão concluída com sucesso!");
    }
}
