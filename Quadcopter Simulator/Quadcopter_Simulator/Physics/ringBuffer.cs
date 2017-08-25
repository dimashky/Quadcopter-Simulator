using MathNet.Numerics.LinearAlgebra;

namespace TripleM.Quadcopter.Physics
{
    class ringBuffer
    {
        private int bufLen;
        private int idx;
        private Vector<float>[] ary;
        public ringBuffer()
        {
            ary = null;
            bufLen = 0;
            idx = 0;
        }
        public ringBuffer(ref Vector<float>[] ary_arg, int bufLen_arg)
        {
            ary = ary_arg;
            bufLen = bufLen_arg;
            idx = 0;
        }

        // ring buffer access functions, return true on success
        public void pushNewElem(Vector<float> newElem)
        {
            ary[idx] = newElem;
            idx = (idx + 1) % bufLen;
        }
        public bool getNthElem(ref Vector<float> x, int n)     // latest element has index 0, then comes 1, 2, 3 ... bufLen-1
        {
            // check if n is valid
            if (n < 0 || n >= bufLen)
                return false;

            // n = 0 means the head element, n = bufLen-1 is the tail element.
            // idx does NOT point to the latest element but to the next index to write to.
            int nthElem = idx - (n + 1);

            // true modulo
            nthElem = (nthElem < 0) ? nthElem + bufLen : nthElem;
	
	        x = ary[nthElem];

            return true;
        }
        public bool getHeadElem(ref Vector<float> x)
        {
            return getNthElem(ref x, 0);
        }
        public bool getPreHeadElem(ref Vector<float> x)
        {
            return getNthElem(ref x, 1);
        }
        public bool getTailElem(ref Vector<float> x)
        {
            return getNthElem(ref x, bufLen - 1);
        }
    }
}
