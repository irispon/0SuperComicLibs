﻿using System;
using SuperComicLib.Collections;

namespace SuperComicLib.CodeDesigner
{
    public delegate void CreateTokenDelegate(ref string text, int line, int row, IAddOnlyList<Token> tokens);

    public delegate void ITypeMapChangedHandler(object sender, ITypeMapChangedArgs args);
}
