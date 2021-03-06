﻿using System;
using System.Collections.Generic;
using ClrHeapAllocationAnalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoslynTestKit;

namespace ClrHeapAllocationsAnalyzer.Test
{
    [TestClass]
    public class AvoidAllocationWithArrayEmptyCodeFixTests: CodeFixTestFixture
    {
        protected override string LanguageName => LanguageNames.CSharp;
        protected override CodeFixProvider CreateProvider() => new AvoidAllocationWithArrayEmptyCodeFix();

        protected override IReadOnlyCollection<DiagnosticAnalyzer> CreateAdditionalAnalyzers() => new DiagnosticAnalyzer[]
        {
            new ExplicitAllocationAnalyzer(), 
        };

        [TestMethod]
        public void should_replace_empty_list_creation_with_array_empty_when_return_from_method_ienumerable()
        {
            var before = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public IEnumerable<int> DoSomething()
        {
            return [|new List<int>()|];
        }
    }
}";
            var after = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public IEnumerable<int> DoSomething()
        {
            return Array.Empty<int>();
        }
    }
}";

            TestCodeFix(before, after, ExplicitAllocationAnalyzer.NewObjectRule.Id, 0);
        } 
        
        [TestMethod]
        public void should_replace_empty_list_creation_with_array_empty_when_return_from_method_ireadonly_list()
        {
            var before = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public IReadOnlyList<int> DoSomething()
        {
            return [|new List<int>()|];
        }
    }
}";
            var after = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public IReadOnlyList<int> DoSomething()
        {
            return Array.Empty<int>();
        }
    }
}";

            TestCodeFix(before, after, ExplicitAllocationAnalyzer.NewObjectRule.Id, 0);
        }
        
        [TestMethod]
        public void should_replace_empty_list_creation_with_array_empty_when_return_from_method_ireadonly_collection()
        {
            var before = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public IReadOnlyCollection<int> DoSomething()
        {
            return [|new List<int>()|];
        }
    }
}";
            var after = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public IReadOnlyCollection<int> DoSomething()
        {
            return Array.Empty<int>();
        }
    }
}";

            TestCodeFix(before, after, ExplicitAllocationAnalyzer.NewObjectRule.Id, 0);
        }        
        
        [TestMethod]
        public void should_replace_empty_list_creation_with_array_empty_when_return_from_method_array()
        {
            var before = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public int[] DoSomething()
        {
            return [|new int[0]|];
        }
    }
}";
            var after = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public int[] DoSomething()
        {
            return Array.Empty<int>();
        }
    }
}";

            TestCodeFix(before, after, ExplicitAllocationAnalyzer.NewArrayRule.Id, 0);
        }
        
        [TestMethod]
        public void should_replace_empty_list_creation_with_array_empty_for_arrow_expression()
        {
            var before = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public IEnumerable<int> DoSomething => [|new List<int>()|];
    }
}";
            var after = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public IEnumerable<int> DoSomething => Array.Empty<int>();
    }
}";

            TestCodeFix(before, after, ExplicitAllocationAnalyzer.NewObjectRule.Id, 0);
        }
        
        [TestMethod]
        public void should_replace_empty_list_creation_with_array_empty_for_readonly_property()
        {
            var before = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public IEnumerable<int> DoSomething { get {return [|new List<int>()|];}}
    }
}";
            var after = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public IEnumerable<int> DoSomething { get {return Array.Empty<int>();}}
    }
}";

            TestCodeFix(before, after, ExplicitAllocationAnalyzer.NewObjectRule.Id, 0);
        }
        
        [TestMethod]
        public void should_replace_empty_list_with_creation_with_predefined_size_with_array_empty()
        {
            var before = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public IEnumerable<int> DoSomething()
        {
            return [|new List<int>(10)|];
        }
    }
}";
            var after = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public IEnumerable<int> DoSomething()
        {
            return Array.Empty<int>();
        }
    }
}";

            TestCodeFix(before, after, ExplicitAllocationAnalyzer.NewObjectRule.Id, 0);
        }
        
        [TestMethod]
        public void should_not_propose_code_fix_when_non_empty_list_created()
        {
            var before = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public IEnumerable<int> DoSomething()
        {
            return [|new List<int>(){1, 2}|];
        }
    }
}";
           
            NoCodeFix(before, ExplicitAllocationAnalyzer.NewObjectRule.Id);
        }
        
        [TestMethod]
        public void should_not_propose_code_fix_when_return_type_inherit_form_enumerable()
        {
            var before = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public List<int> DoSomething()
        {
            return [|new List<int>()|];
        }
    }
}";
           
            NoCodeFix(before, ExplicitAllocationAnalyzer.NewObjectRule.Id);
        }
        
        [TestMethod]
        public void should_not_propose_code_fix_when_for_collection_creation_using_copy_constructor()
        {
            
            var before = @"
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SampleNamespace
{
    class SampleClass
    {
        public IEnumerable<int> DoSomething()
        {
            var innerList = new List<int>(){1, 2};
            return [|new ReadOnlyCollection<int>(innerList)|];
        }
    }
}";
           
            NoCodeFix(before, ExplicitAllocationAnalyzer.NewObjectRule.Id);
        }
        
        [TestMethod]
        public void should_replace_empty_collection_creation_with_array_empty()
        {
            var before = @"
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SampleNamespace
{
    class SampleClass
    {
        public IEnumerable<int> DoSomething()
        {
            return [|new Collection<int>()|];
        }
    }
}";
            var after = @"
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SampleNamespace
{
    class SampleClass
    {
        public IEnumerable<int> DoSomething()
        {
            return Array.Empty<int>();
        }
    }
}";

            TestCodeFix(before, after, ExplicitAllocationAnalyzer.NewObjectRule.Id, 0);
        }
        
        [TestMethod]
        public void should_replace_empty_array_creation_with_array_empty()
        {
            var before = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public IEnumerable<int> DoSomething()
        {
            return [|new int[0]|];
        }
    }
}";
            var after = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public IEnumerable<int> DoSomething()
        {
            return Array.Empty<int>();
        }
    }
}";

            TestCodeFix(before, after, ExplicitAllocationAnalyzer.NewArrayRule.Id, 0);
        }
        
        [TestMethod]
        public void should_not_propose_code_fix_when_non_empty_array_creation()
        {
            var before = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public IEnumerable<int> DoSomething()
        {
            return [|new int[]{1, 2}|];
        }
    }
}";
            NoCodeFix(before,  ExplicitAllocationAnalyzer.NewArrayRule.Id);
        }
        
        [TestMethod]
        public void should_replace_empty_array_creation_with_init_block_with_array_empty()
        {
            var before = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public IEnumerable<int> DoSomething()
        {
            return [|new int[] { }|];
        }
    }
}";
            var after = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public IEnumerable<int> DoSomething()
        {
            return Array.Empty<int>();
        }
    }
}";

            TestCodeFix(before, after, ExplicitAllocationAnalyzer.NewArrayRule.Id, 0);
        }
        
        [TestMethod]
        public void should_replace_list_creation_as_method_invocation_parameter_with_array_empty()
        {
            var before = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public void DoSomething()
        {
            Do([|new List<int>()|]);
        }
        
        private void Do(IEnumerable<int> a)
        {

        }
    }
}";
            var after = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public void DoSomething()
        {
            Do(Array.Empty<int>());
        }
        
        private void Do(IEnumerable<int> a)
        {

        }
    }
}";

            TestCodeFix(before, after, ExplicitAllocationAnalyzer.NewObjectRule.Id, 0);
        }

        [TestMethod]
        public void should_replace_array_creation_as_method_invocation_parameter_with_array_empty()
        {
            var before = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public void DoSomething()
        {
            Do([|new int[0]|]);
        }
        
        private void Do(IEnumerable<int> a)
        {

        }
    }
}";
            var after = @"
using System.Collections.Generic;

namespace SampleNamespace
{
    class SampleClass
    {
        public void DoSomething()
        {
            Do(Array.Empty<int>());
        }
        
        private void Do(IEnumerable<int> a)
        {

        }
    }
}";

            TestCodeFix(before, after, ExplicitAllocationAnalyzer.NewArrayRule.Id, 0);
        }        
        
        [TestMethod]
        public void should_replace_array_creation_as_delegate_invocation_parameter_with_array_empty()
        {
            var before = @"
using System.Collections.Generic;
using System;

namespace SampleNamespace
{
    class SampleClass
    {
        public void DoSomething(Action<IEnumerable<int>> doSth)
        {
            doSth([|new int[0]|]);
        }
    }
}";
            var after = @"
using System.Collections.Generic;
using System;

namespace SampleNamespace
{
    class SampleClass
    {
        public void DoSomething(Action<IEnumerable<int>> doSth)
        {
            doSth(Array.Empty<int>());
        }
    }
}";

            TestCodeFix(before, after, ExplicitAllocationAnalyzer.NewArrayRule.Id, 0);
        }
    }
}
