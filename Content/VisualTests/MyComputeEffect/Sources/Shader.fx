[Begin_ResourceLayout]

	RWStructuredBuffer<uint> Buffer0 : register(u0);

[End_ResourceLayout]

[Begin_Pass:Default]

	[Profile 11_0]
	[Entrypoints CS = CS]
	
	[numthreads(1, 1, 1)]
	void CS(uint3 threadID : SV_DispatchThreadID)
	{
		Buffer0[threadID.x] = 1;
	}

[End_Pass]