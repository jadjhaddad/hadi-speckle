#if ETABS22
using ETABSv1;
#else
using CSiAPIv1;
#endif

namespace DriverCSharp;

class PluginCallback : cPluginCallback
{
  private bool m_IsFinished;
  private int m_ErrorFlag;

  public int ErrorFlag
  {
    get { return m_ErrorFlag; }
  }

  public bool Finished
  {
    get { return m_IsFinished; }
  }

  public void Finish(int iVal)
  {
    m_IsFinished = true;
    m_ErrorFlag = iVal;
  }
}
