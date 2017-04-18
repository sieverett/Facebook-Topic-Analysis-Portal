import React, { Component } from 'react';

class PageControl extends Component {
  static get defaultProps() { return {toShow: 2}; }

  list(pages, key, className, pageNumber, text) {
    // The user can provide a handler for when a page indicator is clicked.
    var clickHandler = event => {
      event.preventDefault();

      // A page indicator can't be clicked if it is disabled.
      if (className !== 'disabled' && this.props.onPageChanged) {
        this.props.onPageChanged(pageNumber);
      }
    };

    var listItem = 
      (<li key={key} className={className}>
        <a href='#' onClick={clickHandler}>{text}</a>
      </li>)
    ;
    pages.push(listItem);
  }

  render() {
    // The user can customize how many items to show on the left and right of the
    // current page indicator.
    var leftIndexEnd = +this.props.pageNumber - this.props.toShow;
    var rightIndexEnd = +this.props.pageNumber + this.props.toShow;

    if (leftIndexEnd < 1) {
      // There is not enough to show on the left hand side. To maintain a constant
      // width of the pagination control, increase the number of page indicators on
      // the right hand side.
      var extraOnRight = 1 - leftIndexEnd;
      rightIndexEnd += extraOnRight;
      leftIndexEnd = 1;
    }
    if (rightIndexEnd > this.props.numberOfPages) {
      // There is not enought to show on the right hand side. To maintain a constant
      // width of the pagination control, increase the number of page indicators on
      // the left hand side.
      leftIndexEnd -= rightIndexEnd - this.props.numberOfPages;
      rightIndexEnd = this.props.numberOfPages;

      // But, if we increased the number of page indicators on the left side, there
      // might not be enough to show on the left hand side again.
      if (leftIndexEnd < 1) {
        leftIndexEnd = 1;
      }
    }

    var pages = [];
 
    this.list(pages, 'First', '', 1, 1);
    
    // Disable the previous page indicator if we're on the first page.
    var previousClass = this.props.pageNumber === 1 ? 'disabled' : '';
    this.list(pages, 'Previous', previousClass, this.props.pageNumber -1, '«');

    // Add all the indicators on the left hand side
    for (var left = leftIndexEnd; left < this.props.pageNumber; left++)
    {
      this.list(pages, left, '', left, left);
    }

    // Add the current indicator in the middle and mark it as active to give it styling.
    this.list(pages, this.props.pageNumber, 'active', this.props.pageNumber, this.props.pageNumber);

    // Add all the indicators on the right hand side.
    for (var right = +this.props.pageNumber + 1; right <= rightIndexEnd; right++)
    {
      this.list(pages, right, '', right, right);
    }

    // Disable the next page indicator if we're on the last page.
    var nextClass = this.props.pageNumber === this.props.numberOfPages ? 'disabled' : ''; 
    this.list(pages, 'Next', nextClass, this.props.pageNumber +1, '»');
    this.list(pages, 'Last', '', this.props.numberOfPages, this.props.numberOfPages);

    return (
      <ul className="pagination">
        {pages}
      </ul>
    );
  }
}

export default PageControl;