#include "stdafx.h"
#include "TestOffendingCppLib.h"

bool TestOffendingCppLib::TesterClass::SelfTest()
{
  return true;
}

void TestOffendingCppLib::TesterClass::ThrowAccessViolation()
{ 
  int* p = (int*)0xFFEEFFEE;
  *p = 1; 
}