namespace bakaTest
{
    // Simple matrix class for inner use
    // Only necessary functionality implemented
    public class Matrix
    {
        private double[][] mtx = null;
        private int nstr, ncol;

        public int StringNumber
        {
            get { return nstr; }
        }

        public int ColumnNumber
        {
            get { return ncol; }
        }

        public Matrix(int _nstr, int _ncol)
        {
            nstr = _nstr;
            ncol = _ncol;

            mtx = new double[nstr][];
            for (int i = 0; i < nstr; ++i)
                mtx[i] = new double[ncol];

            fillZero();
        }

        public void fillZero()
        {
            for (int i = 0; i < nstr; ++i)
                for (int j = 0; j < ncol; ++j)
                    mtx[i][j] = 0;
        }

        public double[] this[int str]
        {
            get { return mtx[str]; }
        }

        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            Matrix res = new Matrix(m1.nstr, m2.ncol);

            for (int i = 0; i < m1.nstr; ++i)
                for (int j = 0; j < m2.ncol; ++j)
                    for (int k = 0; k < m1.ncol; k++)
                        res[i][j] = m1[i][k] * m2[k][j] + res[i][j];

            return res;
        }

        public static Matrix operator -(Matrix m1, Matrix m2)
        {
            Matrix res = new Matrix(m1.nstr, m1.ncol);
            for(int i = 0; i < m1.nstr; ++i)
                for(int j = 0; j < m1.ncol; ++j)
                    res[i][j] = m1[i][j] - m2[i][j];
            return res;
        }

        public void ToConsole()
        {
            for (int i = 0; i < nstr; i++)
            {
                for (int j = 0; j < ncol; j++)
                {
                    System.Console.Write(mtx[i][j] + " ");
                }
                System.Console.WriteLine();
            }
            System.Console.WriteLine();
        }
    }
}