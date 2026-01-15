using Moq;
using Shelly_UI.ViewModels;
using Shelly_UI.Services;
using Shelly_UI.Services.AppCache;
using PackageManager.Alpm;
using Microsoft.Reactive.Testing;
using System.Reactive.Linq;
using System.Reactive;

namespace Shelly_UI.Tests.ViewModels;

public class MainWindowViewModelTests : TestScheduler
{
    private Mock<IConfigService> _configServiceMock;
    private Mock<IAppCache> _appCacheMock;
    private Mock<IAlpmManager> _alpmManagerMock;

    [SetUp]
    public void Setup()
    {
        _configServiceMock = new Mock<IConfigService>();
        _appCacheMock = new Mock<IAppCache>();
        _alpmManagerMock = new Mock<IAlpmManager>();
    }

    [Test]
    public void IsProcessing_ShouldBeFalse_Initially()
    {
        var vm = new MainWindowViewModel(_configServiceMock.Object, _appCacheMock.Object, _alpmManagerMock.Object, this);
        Assert.That(vm.IsProcessing, Is.False);
    }

    [Test]
    public void IsProcessing_ShouldBeTrue_WhenPackageOperationStartEventOccurs()
    {
        var vm = new MainWindowViewModel(_configServiceMock.Object, _appCacheMock.Object, _alpmManagerMock.Object, this);
        
        _alpmManagerMock.Raise(m => m.PackageOperation += null, 
            new AlpmPackageOperationEventArgs(AlpmEventType.PackageOperationStart, "test-package"));

        // Trigger scheduler
        AdvanceBy(1);

        Assert.That(vm.IsProcessing, Is.True);
        Assert.That(vm.ProcessingMessage, Contains.Substring("test-package"));
    }

    [Test]
    public void IsProcessing_ShouldBeFalse_AfterTimeout()
    {
        var vm = new MainWindowViewModel(_configServiceMock.Object, _appCacheMock.Object, _alpmManagerMock.Object, this);
        
        _alpmManagerMock.Raise(m => m.PackageOperation += null, 
            new AlpmPackageOperationEventArgs(AlpmEventType.PackageOperationStart, "test-package"));

        // Trigger scheduler
        AdvanceBy(1);

        Assert.That(vm.IsProcessing, Is.True);
        
        // Advance time by 30 seconds
        AdvanceBy(TimeSpan.FromSeconds(30).Ticks);
        
        Assert.That(vm.IsProcessing, Is.False);
        Assert.That(vm.ProcessingMessage, Is.Empty);
    }

    [Test]
    public void IsProcessing_ShouldNotTimeout_IfNewEventReceived()
    {
        var vm = new MainWindowViewModel(_configServiceMock.Object, _appCacheMock.Object, _alpmManagerMock.Object, this);
        
        _alpmManagerMock.Raise(m => m.PackageOperation += null, 
            new AlpmPackageOperationEventArgs(AlpmEventType.PackageOperationStart, "test-package-1"));

        // Trigger scheduler
        AdvanceBy(1);

        Assert.That(vm.IsProcessing, Is.True);
        
        // Advance time by 29 seconds
        AdvanceBy(TimeSpan.FromSeconds(29).Ticks);
        Assert.That(vm.IsProcessing, Is.True);
        
        // New event
        _alpmManagerMock.Raise(m => m.PackageOperation += null, 
            new AlpmPackageOperationEventArgs(AlpmEventType.PackageOperationStart, "test-package-2"));

        // Trigger scheduler
        AdvanceBy(1);
        
        // Advance time by another 29 seconds (total 58 from start, but only 29 from last event)
        AdvanceBy(TimeSpan.FromSeconds(29).Ticks);
        Assert.That(vm.IsProcessing, Is.True);
        
        // Advance time by another 1 second (total 30 from last event)
        AdvanceBy(TimeSpan.FromSeconds(1).Ticks);
        Assert.That(vm.IsProcessing, Is.False);
    }

    [Test]
    public void IsProcessing_ShouldBeFalse_WhenTransactionDoneEventOccurs()
    {
        var vm = new MainWindowViewModel(_configServiceMock.Object, _appCacheMock.Object, _alpmManagerMock.Object, this);
        
        _alpmManagerMock.Raise(m => m.PackageOperation += null, 
            new AlpmPackageOperationEventArgs(AlpmEventType.TransactionStart, null));

        // Trigger scheduler
        AdvanceBy(1);
        
        _alpmManagerMock.Raise(m => m.PackageOperation += null, 
            new AlpmPackageOperationEventArgs(AlpmEventType.PackageOperationStart, "test-package"));

        // Trigger scheduler
        AdvanceBy(1);
        
        _alpmManagerMock.Raise(m => m.PackageOperation += null, 
            new AlpmPackageOperationEventArgs(AlpmEventType.PackageOperationDone, "test-package"));

        // Trigger scheduler
        AdvanceBy(1);
        
        Assert.That(vm.IsProcessing, Is.True);

        _alpmManagerMock.Raise(m => m.PackageOperation += null, 
            new AlpmPackageOperationEventArgs(AlpmEventType.TransactionDone, null));

        // Trigger scheduler
        AdvanceBy(1);

        Assert.That(vm.IsProcessing, Is.False);
        Assert.That(vm.ProcessingMessage, Is.Empty);
    }

    [Test]
    public void Progress_ShouldUpdate_WhenProgressEventOccurs()
    {
        var vm = new MainWindowViewModel(_configServiceMock.Object, _appCacheMock.Object, _alpmManagerMock.Object, this);

        _alpmManagerMock.Raise(m => m.Progress += null,
            new AlpmProgressEventArgs(AlpmProgressType.AddStart, "test-package", 50, 100, 50));
        
        // Trigger scheduler
        AdvanceBy(1);

        Assert.That(vm.ProgressValue, Is.EqualTo(50));
        Assert.That(vm.ProgressIndeterminate, Is.False);
        Assert.That(vm.ProcessingMessage, Contains.Substring("test-package"));
    }

    [Test]
    public void Progress_ShouldUpdateMessage_WhenPackageNameIsNull()
    {
        var vm = new MainWindowViewModel(_configServiceMock.Object, _appCacheMock.Object, _alpmManagerMock.Object, this);

        _alpmManagerMock.Raise(m => m.Progress += null,
            new AlpmProgressEventArgs(AlpmProgressType.AddStart, null, 75, 100, 75));

        // Trigger scheduler
        AdvanceBy(1);

        Assert.That(vm.ProgressValue, Is.EqualTo(75));
        Assert.That(vm.ProgressIndeterminate, Is.False);
        Assert.That(vm.ProcessingMessage, Contains.Substring("75%"));
    }

    [Test]
    public void Question_ShouldShowPopup_AndSetResponse()
    {
        var vm = new MainWindowViewModel(_configServiceMock.Object, _appCacheMock.Object, _alpmManagerMock.Object, this);
        var args = new AlpmQuestionEventArgs(AlpmQuestionType.InstallIgnorePkg, "Install anyway?");

        // Raise the question event
        _alpmManagerMock.Raise(m => m.Question += null, args);
        AdvanceBy(1);

        // Verify popup is shown
        Assert.That(vm.ShowQuestion, Is.True);
        Assert.That(vm.QuestionText, Is.EqualTo("Install anyway?"));

        // Respond to the question
        vm.RespondToQuestion.Execute("1").Subscribe();
        AdvanceBy(1);

        // Verify popup is hidden and response is set
        Assert.That(vm.ShowQuestion, Is.False);
        Assert.That(args.Response, Is.EqualTo(1));
    }
}
