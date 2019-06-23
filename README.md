# FastHashes.NET - High-performance non-cryptographic hashes

[![NuGet](https://img.shields.io/nuget/v/FastHashesNet.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/FastHashesNet/)

### Hashes

* DJBHash
* FarmHash
* FarshHash
* FNVHash
* HighwayHash
* MurmurHash
* SipHash
* SuperFastHash
* xxHash2

### Performance

Measured on 4 byte keys.

```
                           Method 	       Mean 	     Error 	    StdDev 	 Rank 
        SuperFastHash32UnsafeTest 	  0.7782 ns 	 0.0403 ns 	 0.0377 ns 	    1 
                      FNV1A32Test 	  0.9157 ns 	 0.0161 ns 	 0.0142 ns 	    2 
                    DJBHash32Test 	  0.9695 ns 	 0.0201 ns 	 0.0178 ns 	    3 
              DJBHash32UnsafeTest 	  1.0063 ns 	 0.0460 ns 	 0.0472 ns 	    3 
                FNV1A32UnsafeTest 	  0.9773 ns 	 0.0428 ns 	 0.0400 ns 	    3 
              SuperFastHash32Test 	  1.0867 ns 	 0.0454 ns 	 0.0446 ns 	    4 
                      FNV1A64Test 	  1.1968 ns 	 0.0506 ns 	 0.0622 ns 	    5 
                FNV1A64UnsafeTest 	  1.1583 ns 	 0.0305 ns 	 0.0285 ns 	    5 
           MurmurHash32UnsafeTest 	  1.8314 ns 	 0.0609 ns 	 0.0626 ns 	    6 
            FNV1AWHIZ32UnsafeTest 	  1.9410 ns 	 0.0662 ns 	 0.0679 ns 	    7 
               xxHash32UnsafeTest 	  1.9222 ns 	 0.0398 ns 	 0.0352 ns 	    7 
                     xxHash32Test 	  2.0033 ns 	 0.0660 ns 	 0.0648 ns 	    8 
                 MurmurHash32Test 	  2.1078 ns 	 0.0328 ns 	 0.0274 ns 	    9 
               xxHash64UnsafeTest 	  2.1260 ns 	 0.0673 ns 	 0.0802 ns 	    9 
             Farmhash64UnsafeTest 	  2.3707 ns 	 0.0706 ns 	 0.0725 ns 	   10 
             Farmhash32UnsafeTest 	  2.5665 ns 	 0.0568 ns 	 0.0474 ns 	   11 
                     xxHash64Test 	  2.7029 ns 	 0.0243 ns 	 0.0189 ns 	   12 
                   FarmHash64Test 	  2.9099 ns 	 0.0276 ns 	 0.0244 ns 	   13 
                   FarmHash32Test 	  3.4922 ns 	 0.0938 ns 	 0.0921 ns 	   14 
                  FarshHash64Test 	  3.6338 ns 	 0.0987 ns 	 0.0970 ns 	   15 
            FarshHash64UnsafeTest 	  3.6979 ns 	 0.0327 ns 	 0.0255 ns 	   15 
 FNV1AYoshimitsuTRIAD32UnsafeTest 	  3.6642 ns 	 0.0994 ns 	 0.0977 ns 	   15 
          MurmurHash128UnsafeTest 	  8.5315 ns 	 0.2798 ns 	 0.2617 ns 	   16 
              SipHash64UnsafeTest 	 10.1458 ns 	 0.0839 ns 	 0.0744 ns 	   17 
                    SipHash64Test 	 10.6981 ns 	 0.1657 ns 	 0.1550 ns 	   18 
                MurmurHash128Test 	 35.6638 ns 	 0.6145 ns 	 0.5748 ns 	   19 
```