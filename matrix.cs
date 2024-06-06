using System;

namespace MatrixNamespace
{
    public class Matrix
    {
        private double[,] values;
        private int? hashcode;
        public Matrix(double[,] data)
        {
            values = data;
        }
        public double this[int i, int j]
        {
            get { return values[i, j]; }
        }
        public int Rows
        {
            get { return values.GetLength(0); }
        }
    
        public int Columns
        {
            get { return values.GetLength(1); }
        }
    
        public Matrix Transpose()
        {
            double[,] transposed = new double[Columns, Rows];
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    transposed[j, i] = values[i, j];
                }
            }
            return new Matrix(transposed);
        }
        public override string ToString()
        {
            string result = "";
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    result += $"{values[i, j]} ";
                }
                result += "\n";
            }
            return result;
        }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            Matrix other = (Matrix)obj;
            if (Rows != other.Rows || Columns != other.Columns)
            {
                return false;
            }
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (values[i, j] != other[i, j])
                    {
                        return false;
                    }
                }
            }
    
            return true;
        }
        public override int GetHashCode()
        {
            if (hashcode == null){
                unchecked
                {
                    int hash = 17;
                    foreach (var value in values)
                    {
                        hash = hash * 31 + value.GetHashCode();
                    }
                    hashcode = hash;
                }
            }
            return hashcode.Value;
        }
        public static Matrix operator +(Matrix a) => a;
        public static Matrix operator ~(Matrix a) => a.Transpose();
        public static Matrix operator -(Matrix a){
            double[,] result = new double[a.Rows, a.Columns];
            for (int i=0; i < a.Rows; i++){
                for (int j=0; j < a.Columns; j++){
                    result[i,j] = -a[i,j];
                }
            }
            return new Matrix(result);
        }
        public static Matrix operator +(Matrix a, Matrix b)
        {
            if (a.Rows != b.Rows || a.Columns != b.Columns)
            {
                throw new ArgumentException("МАТРИЦЫ ДОЛЖНЫ БЫТЬ ОДНОГО РАЗМЕРА");
            }
            double[,] result = new double[a.Rows, a.Columns];
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < a.Columns; j++)
                {
                    result[i, j] = a[i, j] + b[i, j];
                }
            }
            return new Matrix(result);
        }
        public static Matrix operator *(Matrix a, Matrix b)
        {
            if (a.Columns != b.Rows)
            {
                throw new ArgumentException("размер столбцов первого должен быть равен строкам второго");
            }
            double[,] result = new double[a.Rows, b.Columns];
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < a.Columns; j++)
                {
                    for (int k = 0; k < a.Columns; k++)
                    {
                        result[i, j] += a[i, k] * b[k, j];
                    }
                }
            }
            return new Matrix(result);
        }
        public static Matrix operator *(Matrix a, double scal)
        {
            double[,] result = new double[a.Rows, a.Columns];
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < a.Columns; j++)
                {
                    result[i, j] = a[i, j] * scal;
                }
            }
            return new Matrix(result);
        }
        public static Matrix operator -(Matrix a, Matrix b)
        {
            if (a.Rows != b.Rows || a.Columns != b.Columns)
            {
                throw new ArgumentException("МАТРИЦЫ ДОЛЖНЫ БЫТЬ ОДНОГО РАЗМЕРА");
            }
            double[,] result = new double[a.Rows, a.Columns];
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < a.Columns; j++)
                {
                    result[i, j] = a[i, j] - b[i, j];
                }
            }
            return new Matrix(result);
        }
        public static Matrix Zero(int rows, int columns)
        {
            double[,] data = new double[rows, columns];
            return new Matrix(data);
        }
        public static Matrix Zero(int n)
        {
            return Zero(n, n);
        }
        public static Matrix Identity(int n)
        {
            double[,] data = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                data[i, i] = 1;
            }
            return new Matrix(data);
        }
        public static Matrix operator /(Matrix a, double scal){
            if (scal ==0) throw new DivideByZeroException("Нельзя на ноль делить");
            return a * (1.0 / scal);
        }
    
    }
}