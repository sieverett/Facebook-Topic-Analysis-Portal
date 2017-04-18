import React, { Component } from 'react';
import Moment from 'react-moment';
import { getPage } from '../Common/Data/Actions';
import DataTable from '../Components/Common/Data/DataTable';
import ErrorPanel from '../Components/Common/ErrorPanel';
import LoadingIndicator from '../Components/Common/LoadingIndicator';
import Panel from '../Components/Common/Panel';
import TextWell from '../Components/Common/Well/TextWell';
import DateWell from '../Components/Common/Well/DateWell';

class PageInformation extends Component {
  state = {}

  componentWillMount() {
    // Load the current page when the page is refreshed or loaded.
    const { pageId } = this.props.params;
    getPage(pageId, (page, errorMessage) => this.setState({page, errorMessage}));
  }

  render() {
    const { page, errorMessage } = this.state;
    const fanCountHistoryMapping = [
      { 'name': 'Date',            'key': path => <Moment format='YYYY-MM-DD HH:mm'>{path.date}</Moment> },
      { 'name': 'Number of Likes', 'key': path => path.fanCount                                          }
    ];

    if (errorMessage) {
      return <ErrorPanel message={errorMessage} />
    } else if (!page) {
      return <LoadingIndicator />
    } 

    return (
      <section>
        <div className="col-md-6">
          <TextWell header={page.name} subheader={(page.fanCount || 0) + ' Likes'} />
          <Panel title="Like History" table>
            <DataTable minSize={10} showIndex={false}
                       mapping={fanCountHistoryMapping} data={page.fanCountHistory} />
          </Panel>
        </div>
        <div className="col-md-6">
          <TextWell header={<a href={'https://facebook.com/' + page.facebookId}>https://facebook.com/{page.facebookId}</a>} />
            <DateWell title="First Scrape" date={page.firstScrape} fallbackTitle="Never Imported" />
            <DateWell title="Latest Scrape" date={page.latestScrape} fallbackTitle="Never Imported" />
            <div className="well">
            <h5 className="text-muted">{page.id}</h5>
          </div>
        </div>
      </section>
    );
  }
}

export default PageInformation;
