
using System.Runtime.CompilerServices;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;
using Veldrid.Utilities;

public class PrimeFinder : ApplicationBase
{
    int[] _numbers;
    DeviceBuffer _numbersBuffer;
    private DeviceBuffer _stagingBuffer;
    CommandList _commandList;
    Shader _shader;
    ResourceLayout _layout;
    Pipeline _pipeline;
    ResourceSet _resourceSet;

    /// <param name="count">up to which number to search for primes</param>
    public PrimeFinder(int count) : base()
    {
        _numbers = Enumerable.Range(1, count).ToArray();
        InitResources();
    }


    protected override void CreateDeviceResources(DisposeCollectorResourceFactory factory)
    {
        _numbersBuffer = factory.CreateBuffer(
            new BufferDescription((uint)(sizeof(int)*_numbers.Length),BufferUsage.StructuredBufferReadWrite,(uint)sizeof(int))
            );
        _stagingBuffer = factory.CreateBuffer(
            new BufferDescription((uint)(sizeof(int)*_numbers.Length),BufferUsage.Staging)
            );
        _commandList = factory.CreateCommandList(new CommandListDescription());
        _shader = factory.CreateFromSpirv(
            new ShaderDescription(
                ShaderStages.Compute,
                EmbeddedResourceReader.ReadBytes("PrimeFinder.comp"),
                "main"
        ));
    }


    protected override void CreatePipelines(DisposeCollectorResourceFactory factory)
    {
        var pipeDesc = new ComputePipelineDescription(_shader,new[]{_layout},1,1,1);
        _pipeline = factory.CreateComputePipeline(ref pipeDesc);
    }

    protected override void CreateResourceLayouts(DisposeCollectorResourceFactory factory)
    {
        _layout = factory.CreateResourceLayout(
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("NumbersBuffer",ResourceKind.StructuredBufferReadWrite,ShaderStages.Compute)
            )
        );
    }

    protected override void CreateResourceSets(DisposeCollectorResourceFactory factory)
    {
        _resourceSet =  factory.CreateResourceSet(
            new ResourceSetDescription(
                _layout,_numbersBuffer
            )
        );
    }

    protected override void UpdateDeviceResources(GraphicsDevice graphicsDevice)
    {
        graphicsDevice.UpdateBuffer(_numbersBuffer,0,_numbers);
    }
    public void Compute()
    {
        _commandList.Begin();
        _commandList.SetPipeline(_pipeline);
        _commandList.SetComputeResourceSet(0,_resourceSet);
        _commandList.Dispatch((uint)_numbers.Length,1,1);

        _commandList.End();
        GraphicsDevice.SubmitCommands(_commandList);
        GraphicsDevice.WaitForIdle();

        _commandList.Begin();
        _commandList.CopyBuffer(_numbersBuffer,0,_stagingBuffer,0,(uint)(_numbers.Length*sizeof(int)));
        _commandList.End();
        GraphicsDevice.SubmitCommands(_commandList);
        GraphicsDevice.WaitForIdle();

        unsafe{
            var mapped = GraphicsDevice.Map(_stagingBuffer,MapMode.Read);
            Span<int> nums = new Span<int>((void*)mapped.Data,_numbers.Length);
            foreach(int n in nums){
                if(n!=0)
                    System.Console.WriteLine(n);
            }
        }
    }
}