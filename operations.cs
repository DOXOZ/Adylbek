using System;
using System.Threading.Tasks;
using System.Threading;

namespace MatrixNamespace
{
    public static class MatrixOperations
    {
        public static Matrix Transpose(Matrix matrix)
        {
            double[,] transposedValues = new double[matrix.Columns, matrix.Rows];
            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Columns; j++)
                {
                    transposedValues[j, i] = matrix[i, j];
                }
            }
            return new Matrix(transposedValues);
        }

        public static Matrix Multiply(Matrix matrix, double scalar)
        {
            double[,] resultValues = new double[matrix.Rows, matrix.Columns];
            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Columns; j++)
                {
                    resultValues[i, j] = matrix[i, j] * scalar;
                }
            }
            return new Matrix(resultValues);
        }

        public static Matrix Add(Matrix a, Matrix b)
        {
            if (a.Rows != b.Rows || a.Columns != b.Columns)
            {
                throw new MatrixOperationException("Matrix dimensions must match for addition.");
            }

            double[,] resultValues = new double[a.Rows, a.Columns];
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < a.Columns; j++)
                {
                    resultValues[i, j] = a[i, j] + b[i, j];
                }
            }
            return new Matrix(resultValues);
        }

        public static Matrix Subtract(Matrix a, Matrix b)
        {
            if (a.Rows != b.Rows || a.Columns != b.Columns)
            {
                throw new MatrixOperationException("Matrix dimensions must match for subtraction.");
            }

            double[,] resultValues = new double[a.Rows, a.Columns];
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < a.Columns; j++)
                {
                    resultValues[i, j] = a[i, j] - b[i, j];
                }
            }
            return new Matrix(resultValues);
        }

        public static Matrix Multiply(Matrix a, Matrix b)
        {
            if (a.Columns != b.Rows)
            {
                throw new MatrixOperationException("Matrix dimensions are not compatible for multiplication.");
            }

            double[,] resultValues = new double[a.Rows, b.Columns];
            Matrix transposedB = Transpose(b);

            Parallel.For(0, a.Rows, i =>
            {
                for (int j = 0; j < b.Columns; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < a.Columns; k++)
                    {
                        sum += a[i, k] * transposedB[j, k];
                    }
                    resultValues[i, j] = sum;
                }
            });

            return new Matrix(resultValues);
        }

        private static double[,] MatrixDecompose(double[,] matrix, out int[] perm, out int toggle)
        {
            int n = matrix.GetLength(0);
            double[,] result = (double[,])matrix.Clone();

            perm = new int[n];
            for (int i = 0; i < n; i++) { perm[i] = i; }
            toggle = 1;

            for (int j = 0; j < n - 1; j++)
            {
                double colMax = Math.Abs(result[j, j]);
                int pRow = j;

                for (int i = j + 1; i < n; i++)
                {
                    if (result[i, j] > colMax)
                    {
                        colMax = result[i, j];
                        pRow = i;
                    }
                }

                if (pRow != j)
                {
                    double[] rowPtr = new double[result.GetLength(1)];
                    for (int k = 0; k < result.GetLength(1); k++)
                    {
                        rowPtr[k] = result[pRow, k];
                        result[pRow, k] = result[j, k];
                        result[j, k] = rowPtr[k];
                    }

                    int tmp = perm[pRow];
                    perm[pRow] = perm[j];
                    perm[j] = tmp;

                    toggle = -toggle;
                }

                double diagVal = result[j, j];
                if (Math.Abs(diagVal) < 1.0E-20)
                {
                    return null;
                }

                for (int i = j + 1; i < n; i++)
                {
                    double mult = result[i, j] / diagVal;
                    result[i, j] = mult;
                    for (int k = j + 1; k < n; k++)
                    {
                        result[i, k] -= mult * result[j, k];
                    }
                }
            }
            return result;
        }

        private static double[] HelperSolve(double[,] luMatrix, double[] b)
        {
            int n = luMatrix.GetLength(0);
            double[] x = new double[n];
            b.CopyTo(x, 0);

            for (int i = 1; i < n; i++)
            {
                double sum = x[i];
                for (int j = 0; j < i; j++)
                {
                    sum -= luMatrix[i, j] * x[j];
                }
                x[i] = sum;
            }

            x[n - 1] = x[n - 1] / luMatrix[n - 1, n - 1];
            for (int i = n - 2; i >= 0; i--)
            {
                double sum = x[i];
                for (int j = i + 1; j < n; j++)
                {
                    sum -= luMatrix[i, j] * x[j];
                }
                x[i] = sum / luMatrix[i, i];
            }

            return x;
        }
    }

    public class MatrixOperationException : Exception
    {
        public MatrixOperationException(string message) : base(message)
        {
        }
    }
}
