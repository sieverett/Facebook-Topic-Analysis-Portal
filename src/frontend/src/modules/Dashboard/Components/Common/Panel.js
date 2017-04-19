import React, { Component } from 'react';

class PanelHeading extends Component {
  handleOnClick = (e) => { e.preventDefault(); this.props.onClick(); }

  render() {
    return (
      <div className="clearfix">
        <h4 className="pull-left">{this.props.title}</h4>
        <a className="btn btn-default pull-right" onClick={this.handleOnClick}>{this.props.buttonTitle}</a>
      </div>
    );
  }
}

class Panel extends Component {
  static get defaultProps() { return {showHeading: true}; }

  render() {
    let heading;
    if (this.props.showHeading) {
      heading = (
        <div className="panel-heading">
          {this.props.heading || <span><strong>{this.props.title}</strong>{this.props.heading}</span>}
        </div>
      );
    }

    let body;
    if (this.props.table) {
      body = this.props.children;
    } else {
      body = <div className="panel-body">{this.props.children}</div>;
    }

    return (
      <section className={this.props.className}>
        <div className={'panel panel-default ' + (this.props.table ? 'table-panel' : '')}>
          {heading}
          {body}
        </div>
      </section>
    );
  }
}

export default Panel;
export { PanelHeading };
