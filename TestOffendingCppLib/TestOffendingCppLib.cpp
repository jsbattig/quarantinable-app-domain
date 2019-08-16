#include "stdafx.h"
#include "TestOffendingCppLib.h"

bool TestOffendingCppLib::TesterClass::SelfTest()
{
  Holder::callsCount++;
  return true;
}

void TestOffendingCppLib::TesterClass::ThrowAccessViolation()
{ 
  Holder::callsCount++;
  int* p = (int*)0xFFEEFFEE;
  *p = 1; 
}

int TestOffendingCppLib::TesterClass::SelfTestCallsCount()
{
  return Holder::callsCount;
}