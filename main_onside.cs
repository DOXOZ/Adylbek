using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MatrixNamespace;

namespace MatrixProgram
{
    class Program2
    {
        static async Task Main(string[] args)
        {
            var a = Enumerable.Range(0, 50).Select(_ => CreateRandomMatrix(500, 100)).ToArray();
            var b = Enumerable.Range(0, 50).Select(_ => CreateRandomMatrix(100, 500)).ToArray();

            string resultsDir = "Results";
            if (Directory.Exists(resultsDir))
                Directory.Delete(resultsDir, true);
            Directory.CreateDirectory(resultsDir);

            var tasks = new[]
            {
                Task.Run(() => MultiplyAndSaveResults(a, b, "Results/ab_multiplication.tsv")),
                Task.Run(() => MultiplyAndSaveResults(b, a, "Results/ba_multiplication.tsv")),
                Task.Run(() => ScalarProductAndSaveResults(a, b, "Results/ab_scalar.tsv")),
                Task.Run(() => ScalarProductAndSaveResults(b, a, "Results/ba_scalar.tsv"))
            };

            await Task.WhenAll(tasks);

            string[] formatDirs = { "BinaryFormat", "TextFormat", "JsonFormat" };
            foreach (var dir in formatDirs)
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
                Directory.CreateDirectory(dir);
            }

            var saveTasks = new[]
            {
                Task.Run(() => SaveMatricesAsync(a, "TextFormat", "a_", "csv", MatrixIO.WriteMatrixToFileAsync)),
                Task.Run(() => SaveMatricesAsync(b, "TextFormat", "b_", "csv", MatrixIO.WriteMatrixToFileAsync)),
                Task.Run(() => SaveMatricesAsync(a, "JsonFormat", "a_", "json", MatrixIO.WriteMatrixJsonAsync)),
                Task.Run(() => SaveMatricesAsync(b, "JsonFormat", "b_", "json", MatrixIO.WriteMatrixJsonAsync))
            };

            await Task.WhenAll(saveTasks);
            Console.WriteLine("Запись в директории завершена.");

            // async reading from text files
            var readTextTask = Task.Run(() => ReadMatricesAsync("TextFormat", "a_", "csv", MatrixIO.ReadMatrixFromFileAsync));
            var readJsonTask = Task.Run(() => ReadMatricesAsync("JsonFormat", "a_", "json", MatrixIO.ReadMatrixJsonAsync));

            var readCompletedTask = await Task.WhenAny(readTextTask, readJsonTask);
            string format = readCompletedTask == readTextTask ? "text" : "json";
            Console.WriteLine($"Чтение завершено: {format}");

            var readMatrices = await readCompletedTask;

            // Сравнение матриц
            var compareTask = Task.Run(() => CompareMatrixArrays(a, readMatrices));
            bool comparisonResult = await compareTask;
            Console.WriteLine($"Сравнение массивов: {comparisonResult}");

            // Синхронное сохранение, чтение бинарных файлов
            SaveMatricesSync(a, "BinaryFormat", "a_", "bin", MatrixIO.WriteMatrixBinary);
            SaveMatricesSync(b, "BinaryFormat", "b_", "bin", MatrixIO.WriteMatrixBinary);
            Console.WriteLine("Запись бинарных файлов завершена.");

            var readBinaryMatrices = ReadMatricesSync("BinaryFormat", "a_", "bin", MatrixIO.ReadMatrixBinary);
            var compareBinaryTask = Task.Run(() => CompareMatrixArrays(a, readBinaryMatrices));
            bool binaryComparisonResult = await compareBinaryTask;
            Console.WriteLine($"Сравнение бинарных массивов: {binaryComparisonResult}");
        }

        public static Matrix CreateRandomMatrix(int rows, int cols)
        {
            Random rand = new Random();
            double[,] values = new double[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    values[i, j] = rand.NextDouble() * 20 - 10; 
                }
            }
            return new Matrix(values);
        }

        public static void MultiplyAndSaveResults(Matrix[] first, Matrix[] second, string outputPath)
        {
            var results = MultiplyMatricesSequentially(first, second);
            using (var writer = new StreamWriter(outputPath))
            {
                for (int i = 0; i < results.Length; i++)
                {
                    writer.WriteLine($"Matrix {i + 1}:\n{results[i]}");
                }
            }
        }

        public static void ScalarProductAndSaveResults(Matrix[] first, Matrix[] second, string outputPath)
        {
            var result = CalculateScalarProduct(first, second);
            using (var writer = new StreamWriter(outputPath))
            {
                writer.WriteLine(result);
            }
        }

        public static async Task SaveMatricesAsync(Matrix[] matrices, string directory, string prefix, string extension, Func<Matrix, Stream, Task> writeMethod)
        {
            for (int i = 0; i < matrices.Length; i++)
            {
                string filePath = Path.Combine(directory, $"{prefix}{i}.{extension}");
                await MatrixIO.WriteMatrixToFileAsync(directory, $"{prefix}{i}.{extension}", matrices[i], writeMethod);
            }
        }

        public static void SaveMatricesSync(Matrix[] matrices, string directory, string prefix, string extension, Action<Matrix, Stream> writeMethod)
        {
            for (int i = 0; i < matrices.Length; i++)
            {
                string filePath = Path.Combine(directory, $"{prefix}{i}.{extension}");
                MatrixIO.WriteMatrixToFile(directory, $"{prefix}{i}.{extension}", matrices[i], writeMethod);
            }
        }

        public static async Task<Matrix[]> ReadMatricesAsync(string directory, string prefix, string extension, Func<Stream, Task<Matrix>> readMethod)
        {
            var files = Directory.GetFiles(directory, $"{prefix}*.{extension}");
            Matrix[] matrices = new Matrix[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                int index = int.Parse(Path.GetFileNameWithoutExtension(files[i]).Substring(prefix.Length));
                matrices[index] = await MatrixIO.ReadMatrixFromFileAsync(files[i], readMethod);
            }

            return matrices;
        }

        public static Matrix[] ReadMatricesSync(string directory, string prefix, string extension, Func<Stream, Matrix> readMethod)
        {
            var files = Directory.GetFiles(directory, $"{prefix}*.{extension}");
            Matrix[] matrices = new Matrix[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                int index = int.Parse(Path.GetFileNameWithoutExtension(files[i]).Substring(prefix.Length));
                matrices[index] = MatrixIO.ReadMatrixFromFile(files[i], readMethod);
            }

            return matrices;
        }

        public static Matrix[] MultiplyMatricesSequentially(Matrix[] first, Matrix[] second)
        {
            if (first.Length != second.Length)
            {
                throw new ArgumentException("Arrays must be of equal length");
            }

            Matrix[] result = new Matrix[first.Length];
            for (int i = 0; i < first.Length; i++)
            {
                result[i] = MatrixOperations.Multiply(first[i], second[i]);
            }

            return result;
        }

        public static Matrix CalculateScalarProduct(Matrix[] first, Matrix[] second)
        {
            if (first.Length != second.Length)
            {
                throw new ArgumentException("Arrays must be of equal length");
            }

            Matrix sum = Matrix.Zero(first[0].Rows, first[0].Columns);
            for (int i = 0; i < first.Length; i++)
            {
                sum = MatrixOperations.Add(sum, MatrixOperations.Multiply(first[i], second[i]));
            }

            return sum;
        }

        public static bool CompareMatrixArrays(Matrix[] first, Matrix[] second)
        {
            if (first.Length != second.Length)
            {
                return false;
            }

            for (int i = 0; i < first.Length; i++)
            {
                if (!first[i].Equals(second[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
