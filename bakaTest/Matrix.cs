namespace bakaTest
{
    // Simple matrix class for inner use
    // Only necessary functionality implemented
    public class Matrix
    {
        private double[][] mtx = null;
        private int nstr, ncol;
        private int p;
        private int p_2;

        public Matrix(int nstr_, int ncol_)
        {
            nstr = nstr_;
            ncol = ncol_;

            mtx = new double[nstr][];
            for (int i = 0; i < nstr; ++i)
                mtx[i] = new double[ncol];

            zfill();
        }

        public void zfill()
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
    }
}