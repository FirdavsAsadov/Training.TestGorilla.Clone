﻿
namespace TestGorilla.Domain;
public abstract class Auditable
{
    public long Id { get; set; }
    public DateTime CratedTime { get; set; }
    public DateTime UpdateTime { get; set; }

}