using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MatrixNamespace;

namespace MatrixProgram
{
    public class Program
    {
        static void Main(string[] args)
        {
            var matrices = new Matrix[]
            {
                CreateRandomMatrix(3, 3),
                CreateRandomMatrix(3, 3)
            };

            var matrices2 = new Matrix[]
            {
                CreateRandomMatrix(3, 3),
                CreateRandomMatrix(3, 3)
            };

            var result = MultiplyMatricesSequentially(matrices, matrices2);
            Console.WriteLine("Multiplication result:");
            foreach (var matrix in result)
            {
                Console.WriteLine(matrix);
            }

            var scalarResult = CalculateScalarProduct(matrices, matrices2);
            Console.WriteLine("Scalar product result:");
            Console.WriteLine(scalarResult);
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



        public static void WriteMatricesToDirectory(Matrix[] matrices, string directory, string prefix, string extension, Action<Matrix, Stream> writeMethod)
        {
            for (int i = 0; i < matrices.Length; i++)
            {
                string filePath = Path.Combine(directory, $"{prefix}{i}.{extension}");
                MatrixIO.WriteMatrixToFile(directory, $"{prefix}{i}.{extension}", matrices[i], writeMethod);
                if (i % 10 == 0)
                {
                    Console.WriteLine($"Written {i} matrices.");
                }
            }
        }


        public static async Task WriteMatricesToDirectoryAsync(Matrix[] matrices, string directory, string prefix, string extension, Func<Matrix, Stream, Task> writeMethod)
        {
            for (int i = 0; i < matrices.Length; i++)
            {
                string filePath = Path.Combine(directory, $"{prefix}{i}.{extension}");
                await MatrixIO.WriteMatrixToFileAsync(directory, $"{prefix}{i}.{extension}", matrices[i], writeMethod);
                if (i % 10 == 0)
                {
                    Console.WriteLine($"Written {i} matrices.");
                }
            }
        }

        public static Matrix[] ReadMatricesFromDirectory(string directory, string prefix, string extension, Func<Stream, Matrix> readMethod)
        {
            var files = Directory.GetFiles(directory, $"{prefix}*.{extension}");
            Matrix[] matrices = new Matrix[files.Length];

            foreach (var file in files)
            {
                int index = int.Parse(Path.GetFileNameWithoutExtension(file).Substring(prefix.Length));
                matrices[index] = MatrixIO.ReadMatrixFromFile(file, readMethod);
            }

            return matrices;
        }
        public static async Task<Matrix[]> ReadMatricesFromDirectoryAsync(string directory, string prefix, string extension, Func<Stream, Task<Matrix>> readMethod){
            var files = Directory.GetFiles(directory, $"{prefix}*.{extension}");
            Matrix[] matrices = new Matrix[files.Length];

            foreach (var file in files){
                int index = int.Parse(Path.GetFileNameWithoutExtension(file).Substring(prefix.Length));
                matrices[index] = await MatrixIO.ReadMatrixFromFileAsync(file, readMethod);
            }

            return matrices;
        }
        public static bool CompareMatrixArrays(Matrix[] first, Matrix[] second){
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
