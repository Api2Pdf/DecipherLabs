﻿namespace RaythaZero.Application.Common.Interfaces;

public interface IRenderEngine
{
    string RenderAsHtml(string template, object entity);
}
