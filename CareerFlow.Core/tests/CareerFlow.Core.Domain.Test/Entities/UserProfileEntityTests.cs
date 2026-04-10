using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.ValueObjects;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Domain.Test;

public class UserProfileEntityTests
{
    private static readonly LearningType ValidLearning = LearningType.Visual;
    private static readonly List<UserType> ValidTypes = [UserType.Student];
 
    private static UserProfile Profile(Guid? id = null) =>
        UserProfile.Create(id ?? Guid.NewGuid(), ValidLearning, ValidTypes, "Student");
 
    private static SubChapter Sub() => SubChapter.Create("Sub", "Summary", "<p>x</p>");
    private static Chapter Chap(int day = 1) => Chapter.Create(day, "Title", "Core", [Sub()]);
    private static Course MakeCourse() => Course.Create("Topic", [Chap()]);
 
    [Fact]
    public void Create_ValidParameters_ReturnsProfile()
    {
        var id = Guid.NewGuid();
 
        var profile = UserProfile.Create(id, ValidLearning, ValidTypes, "Student");
 
        profile.ShouldNotBeNull();
        profile.AccountId.ShouldBe(id);
        profile.Domain.ShouldBe("Student");
        profile.Experience.ShouldBe(0);
        profile.CorrectAnswersForQuiz.ShouldBe(0);
        profile.IncorrectAnswersForQuiz.ShouldBe(0);
    }
 
    [Fact]
    public void Create_NullDomain_DefaultsToStudent()
    {
        var profile = UserProfile.Create(Guid.NewGuid(), ValidLearning, ValidTypes, null);
 
        profile.Domain.ShouldBe("Student");
    }
 
    [Fact]
    public void Create_EmptyAccountId_ThrowsInvalidFieldException()
    {
        Should.Throw<InvalidFieldException>(() =>
            UserProfile.Create(Guid.Empty, ValidLearning, ValidTypes));
    }
 
    [Fact]
    public void Create_NullLearningType_ThrowsInvalidFieldException()
    {
        Should.Throw<InvalidFieldException>(() =>
            UserProfile.Create(Guid.NewGuid(), null!, ValidTypes));
    }
 
    [Fact]
    public void Create_NullUserTypes_ThrowsInvalidFieldException()
    {
        Should.Throw<InvalidFieldException>(() =>
            UserProfile.Create(Guid.NewGuid(), ValidLearning, null!));
    }
 
    [Fact]
    public void Create_EmptyUserTypes_ThrowsInvalidFieldException()
    {
        Should.Throw<InvalidFieldException>(() =>
            UserProfile.Create(Guid.NewGuid(), ValidLearning, []));
    }
 
    [Fact]
    public void Update_ValidParameters_UpdatesProfile()
    {
        var profile = Profile();
 
        profile.Update(LearningType.Auditory, [UserType.JobSearcher], "Medicine");
 
        profile.Domain.ShouldBe("Medicine");
        profile.UserTypes.ShouldHaveSingleItem();
    }
 
    [Fact]
    public void Update_NullLearningType_ThrowsInvalidFieldException()
    {
        var profile = Profile();
 
        Should.Throw<InvalidFieldException>(() =>
            profile.Update(null!, ValidTypes, "Student"));
    }
 
    [Fact]
    public void Update_NullUserTypes_ThrowsInvalidFieldException()
    {
        var profile = Profile();
 
        Should.Throw<InvalidFieldException>(() =>
            profile.Update(ValidLearning, null!, "Student"));
    }
 
    [Fact]
    public void Update_EmptyUserTypes_ThrowsInvalidFieldException()
    {
        var profile = Profile();
 
        Should.Throw<InvalidFieldException>(() =>
            profile.Update(ValidLearning, [], "Student"));
    }
 
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Update_InvalidDomain_ThrowsInvalidFieldException(string? domain)
    {
        var profile = Profile();
 
        Should.Throw<InvalidFieldException>(() =>
            profile.Update(ValidLearning, ValidTypes, domain!));
    }
 
    [Fact]
    public void Update_ReplacesUserTypes()
    {
        var profile = Profile();
        var newTypes = new List<UserType> { UserType.JobSearcher, UserType.HobbyLearner };
 
        profile.Update(ValidLearning, newTypes, "Student");
 
        profile.UserTypes.Count.ShouldBe(2);
        profile.UserTypes.ShouldNotContain(UserType.Student);
    }
 
    [Fact]
    public void EnrollInCourse_ValidCourse_AddsCourse()
    {
        var profile = Profile();
        var course = MakeCourse();
 
        profile.EnrollInCourse(course);
 
        profile.Courses.ShouldHaveSingleItem();
    }
 
    [Fact]
    public void EnrollInCourse_NullCourse_ThrowsInvalidFieldException()
    {
        var profile = Profile();
 
        Should.Throw<InvalidFieldException>(() => profile.EnrollInCourse(null!));
    }
 
    [Fact]
    public void EnrollInCourse_AlreadyEnrolled_ThrowsInvalidFieldException()
    {
        var profile = Profile();
        var course = MakeCourse();
        profile.EnrollInCourse(course);
 
        Should.Throw<InvalidFieldException>(() => profile.EnrollInCourse(course));
    }
 
    [Fact]
    public void UnenrollFromCourse_EnrolledCourse_RemovesCourse()
    {
        var profile = Profile();
        var course = MakeCourse();
        profile.EnrollInCourse(course);
 
        profile.UnenrollFromCourse(course.Id);
 
        profile.Courses.ShouldBeEmpty();
    }
 
    [Fact]
    public void UnenrollFromCourse_NotEnrolled_ThrowsInvalidFieldException()
    {
        var profile = Profile();
 
        Should.Throw<InvalidFieldException>(() =>
            profile.UnenrollFromCourse(Guid.NewGuid()));
    }
 
    [Fact]
    public void FinishChapter_NewChapter_AddsToFinishedChapters()
    {
        var profile = Profile();
        var chapterId = Guid.NewGuid().ToString();
 
        profile.FinishChapter(chapterId);
 
        profile.FinishedChapters.ShouldContain(chapterId);
    }
 
    [Fact]
    public void FinishChapter_NewChapter_IncreasesExperienceBy10()
    {
        var profile = Profile();
 
        profile.FinishChapter(Guid.NewGuid().ToString());
 
        profile.Experience.ShouldBe(10);
    }
 
    [Fact]
    public void FinishChapter_ThreeChapters_AccumulatesExperience()
    {
        var profile = Profile();
 
        profile.FinishChapter(Guid.NewGuid().ToString());
        profile.FinishChapter(Guid.NewGuid().ToString());
        profile.FinishChapter(Guid.NewGuid().ToString());
 
        profile.Experience.ShouldBe(30);
    }
 
    [Fact]
    public void FinishChapter_AlreadyFinished_ThrowsInvalidFieldException()
    {
        var profile = Profile();
        var chapterId = Guid.NewGuid().ToString();
        profile.FinishChapter(chapterId);
 
        Should.Throw<InvalidFieldException>(() => profile.FinishChapter(chapterId));
    }
 
    [Fact]
    public void EnrollMultipleCourses_AddsAll()
    {
        var profile = Profile();
 
        profile.EnrollInCourse(MakeCourse());
        profile.EnrollInCourse(MakeCourse());
 
        profile.Courses.Count.ShouldBe(2);
    }
}
 