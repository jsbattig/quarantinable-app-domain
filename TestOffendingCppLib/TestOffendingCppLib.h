#pragma once

using namespace System;

namespace TestOffendingCppLib {
  public ref class TesterClass
  {
  public:
    static TesterClass()
    {

    }

    bool SelfTest();
    void ThrowAccessViolation();
  };
}
