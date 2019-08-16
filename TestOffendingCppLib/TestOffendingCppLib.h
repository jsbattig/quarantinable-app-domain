#pragma once

using namespace System;

namespace TestOffendingCppLib {
  public ref class Holder
  {
  public:
    static int callsCount = 0;
  };

  [Serializable]
  public ref class TesterClass : MarshalByRefObject
  {    
  public:
    bool SelfTest();
    void ThrowAccessViolation();
    int SelfTestCallsCount();
  };
}