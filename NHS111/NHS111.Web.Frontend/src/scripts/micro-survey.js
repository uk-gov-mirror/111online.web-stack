

jQuery(document).ready(function () {
  var questions = {}
  var embeddedData = {}
  var currentQuestionID = "QID1";
  var previousQuestionID = null;

  var questionsAnswered = {};

  function displayQuestion(questionID) {
    currentQuestionID = questionID;

    var title = questions[questionID].title;
    var choices = questions[questionID].choices;


    $("#microSurveyQuestions").empty();

    $("#microSurveyQuestions").append('<p id="questionTitle"><strong>' + title + '</strong></p>');

    choices.forEach(function (choice) {
      var choiceWithNoSpaces = choice.choiceText.split(" ").join("");
      var choiceNumber = choice.choiceId;
      var inputType = choice.inputType;

      if (choice.showIfServiceType && !choice.showIfServiceType.includes(embeddedData.serviceType)) return;

      $("#microSurveyQuestions")
        .append(`
            <input type="${inputType}" id="${choiceWithNoSpaces}" name="choice" value="${choiceNumber}">
            <label id="${choiceWithNoSpaces}Label" for="${choiceWithNoSpaces}">
                ${choice.choiceText}
            </label>
            <br>\r\n
        `);
    });

    $("#microSurveyQuestions")
      .append(`<button class="button--next" id="microSurveyNext" type="button" name="Next" value="Next">Next</button>\r\n`);

    if (currentQuestionID != "QID1") {
      $("#microSurveyQuestions").append(`<br><br><button class="button--link" id="microSurveyPrevious" type="button">Change my previous answer</button>\r\n`);
    }

  }

  function displayThanksForYourFeedback() {
    $("#microSurveyQuestions").hide();
    $("#microSurveyEnd").show();
  }

  function displayYouMustSelectOneOption() {
    if ($("#onlySelectOneOption").length === 0) {
      $("#questionTitle")
        .after(
          '<p class="field-validation-error error-message" id="onlySelectOneOption">You must select one option</p>');
    }
  }

  function postSurveyAnswers(data) {
    $.ajax({
      url: '/microsurvey/PostRecommendedServiceSurvey',
      method: 'POST',
      contentType: "application/json",
      data: JSON.stringify(data),
      error: function (resultData) {
        logEvent(EventTypes.Error, "Micro survey failed")
      }
    });
  };


  function getAnswersForQuestion(questionID) {

    var question = questions[questionID];
    var selectedChoices = $("#microSurveyQuestions input:checked");

    if (selectedChoices.length === 0) return false

    if (question.answerType === "Number") {
      return selectedChoices.attr("value")
    }

    if (question.answerType === "[Number]") {
      var choiceIds = [];

      selectedChoices.each(function () {
        choiceIds.push($(this).attr('value'));
      });

      return choiceIds;
    }
  }

  function getNextQuestionID(questionID, answers) {
    var answersArray = answers instanceof Array ? answers : [answers];
    var question = questions[questionID];

    var nextQuestionID;
    answersArray.forEach((answer) => {
      var filtered = question.choices.filter((choice) => choice.choiceId === Number(answer))
      if (filtered.length && filtered[0].nextQuestionID) nextQuestionID = filtered[0].nextQuestionID
    })

    return nextQuestionID
  }

  // Listeners
  window.startMicroSurvey = function (_questions, _embeddedData) {

    $("#microSurveyLink").hide()
    $("#microSurveyQuestions").show()

    questions = _questions
    embeddedData = _embeddedData
    displayQuestion("QID1");

    $("#microSurveyQuestions").on('click', '#microSurveyPrevious', function () {
      if (previousQuestionID) displayQuestion(previousQuestionID);
    });

    $("#microSurveyQuestions").on('click', '#microSurveyNext', function () {

      var answers = getAnswersForQuestion(currentQuestionID);
      if (!answers) {
        displayYouMustSelectOneOption();
        return;
      }

      questionsAnswered[currentQuestionID] = answers;

      previousQuestionID = currentQuestionID;

      var nextQuestionID = getNextQuestionID(currentQuestionID, answers)
      if (nextQuestionID) {
        displayQuestion(nextQuestionID)
      }
      else {

        // Displays thank you regardless of success or failure of submitting micro survey
        displayThanksForYourFeedback();

        postSurveyAnswers({
          "values": JSON.stringify(questionsAnswered)
        });
      }
    });

  }

});
